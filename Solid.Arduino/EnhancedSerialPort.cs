// /*
// Copyright 2013 Antanas Veiverys antanas.veiverys.com
//
// Refactored 2014 by Henk van Boeijen.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// */
//

using System;
using System.IO.Ports;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Threading;

namespace Solid.Arduino
{
    /// <summary>
    /// Represents a system serial port, supporting .NET and Mono.
    /// </summary>
    /// <remarks>
    /// This class is a workaround for Mono's <see cref="SerialPort"/> implementation of event <see cref="OnDataReceived"/>.
    /// <para>
    /// Copyright 2013 Antanas Veiverys <seealso href="https://antanas.veiverys.com">antanas.veiverys.com</seealso>
    /// </para>
    /// </remarks>
    /// <inheritdoc cref="SerialPort" />
    public class EnhancedSerialPort : SerialPort
    {

        [DllImport("MonoPosixHelper", SetLastError = true)]
        private static extern bool poll_serial(int fd, out int error, int timeout);

        [DllImport("libc")]
        private static extern IntPtr strerror(int errnum);

        #region Private Fields

        // Private member access through reflection.
        private int _fdStreamField;
        private FieldInfo _disposedFieldInfo;
        private object _dataReceived;

        #endregion

        #region Constructors

        /// <inheritdoc cref="SerialPort()"/>
        public EnhancedSerialPort()
        {
        }

        /// <inheritdoc cref="SerialPort(IContainer)"/>
        public EnhancedSerialPort(IContainer container)
            : base(container)
        {
        }

        /// <inheritdoc cref="SerialPort(string)"/>
        public EnhancedSerialPort(string portName)
            : this(portName, 9600, Parity.None, 8, StopBits.One)
        {
        }

        /// <inheritdoc cref="SerialPort(string,int)"/>
        public EnhancedSerialPort(string portName, int baudRate)
            : this(portName, baudRate, Parity.None, 8, StopBits.One)
        {
        }

        /// <inheritdoc cref="SerialPort(string,int,Parity)"/>
        public EnhancedSerialPort(string portName, int baudRate, Parity parity)
            : this(portName, baudRate, parity, 8, StopBits.One)
        {
        }

        /// <inheritdoc cref="SerialPort(string,int,Parity,int)"/>
        public EnhancedSerialPort(string portName, int baudRate, Parity parity, int dataBits)
            : this(portName, baudRate, parity, dataBits, StopBits.One)
        {
        }

        /// <inheritdoc cref="SerialPort(string,int,Parity,int,StopBits)"/>
        public EnhancedSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(GetInitializedPortName(portName), baudRate, parity, dataBits, stopBits)
        {
        }

        private static string GetInitializedPortName(string portName)
        {
            if (IsWindows)
                SerialPortFixer.Initialize(portName);

            return portName;
        }

        #endregion

        #region Public Methods

        /// <inheritdoc cref="SerialPort.Open"/>
        public new void Open()
        {
            base.Open();

            if (IsWindows) return;

            FieldInfo fieldInfo = BaseStream.GetType().GetField("fd", BindingFlags.Instance | BindingFlags.NonPublic);

            if (fieldInfo != null)
            {
                _fdStreamField = (int) fieldInfo.GetValue(BaseStream);
                _disposedFieldInfo = BaseStream.GetType()
                    .GetField("disposed", BindingFlags.Instance | BindingFlags.NonPublic);
                fieldInfo = typeof (SerialPort).GetField("data_received", BindingFlags.Instance | BindingFlags.NonPublic);

                if (fieldInfo != null)
                    _dataReceived = fieldInfo.GetValue(this);
            }

            new Thread(EventThreadFunction).Start();
        }

        #endregion

        #region Private Methods

        private static bool IsWindows
        {
            get
            {
                PlatformID id = Environment.OSVersion.Platform;
                return id == PlatformID.Win32Windows || id == PlatformID.Win32NT; // WinCE not supported
            }
        }

        private void EventThreadFunction()
        {
            do
            {
                try
                {
                    var stream = BaseStream;

                    if (stream == null)
                    {
                        return;
                    }
                    if (Poll(stream))
                    {
                        OnDataReceived(null);
                    }
                }
                catch
                {
                    return;
                }
            }
            while (IsOpen);
        }

        private void OnDataReceived(SerialDataReceivedEventArgs args)
        {
            ((SerialDataReceivedEventHandler)Events[_dataReceived])?.Invoke(this, args);
        }

        private bool Poll(Stream stream)
        {
            CheckDisposed(stream);

            if (IsOpen == false)
            {
                throw new InvalidOperationException("Port is closed.");
            }

            bool pollResult = poll_serial(_fdStreamField, out int error, ReadTimeout);

            if (error != -1)
                return pollResult;

            int errnum = Marshal.GetLastWin32Error();
            string errorMessage = Marshal.PtrToStringAnsi(strerror(errnum));

            throw new IOException(errorMessage);
        }

        private void CheckDisposed(Stream stream)
        {
            bool disposed = (bool)_disposedFieldInfo.GetValue(stream);

            if (disposed)
            {
                throw new ObjectDisposedException(stream.GetType().FullName);
            }
        }

        #endregion
    }
}
