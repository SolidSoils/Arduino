// /*
// Copyright 2013 Antanas Veiverys www.veiverys.com
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

namespace Solid.Arduino
{
    /// <summary>
    /// Represents a system serial port. This class is workaround for Mono's SerialPort implementation of event OnDataReceived
    /// </summary>
    public class EnhancedSerialPort : SerialPort
    {
        public EnhancedSerialPort()
            : base()
        {
        }

        public EnhancedSerialPort(IContainer container)
            : base(container)
        {
        }

        public EnhancedSerialPort(string portName)
            : base(portName)
        {
        }

        public EnhancedSerialPort(string portName, int baudRate)
            : base(portName, baudRate)
        {
        }

        public EnhancedSerialPort(string portName, int baudRate, Parity parity)
            : base(portName, baudRate, parity)
        {
        }

        public EnhancedSerialPort(string portName, int baudRate, Parity parity, int dataBits)
            : base(portName, baudRate, parity, dataBits)
        {
        }

        public EnhancedSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {
        }

        // private member access via reflection
        int fd;
        FieldInfo disposedFieldInfo;
        object data_received;

        public new void Open()
        {
            base.Open();

            if (IsWindows == false)
            {
                FieldInfo fieldInfo = BaseStream.GetType().GetField("fd", BindingFlags.Instance | BindingFlags.NonPublic);
                fd = (int)fieldInfo.GetValue(BaseStream);
                disposedFieldInfo = BaseStream.GetType().GetField("disposed", BindingFlags.Instance | BindingFlags.NonPublic);
                fieldInfo = typeof(SerialPort).GetField("data_received", BindingFlags.Instance | BindingFlags.NonPublic);
                data_received = fieldInfo.GetValue(this);

                new System.Threading.Thread(this.EventThreadFunction).Start();
            }
        }

        static bool IsWindows
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
                    var _stream = BaseStream;
                    if (_stream == null)
                    {
                        return;
                    }
                    if (Poll(_stream, ReadTimeout))
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

        void OnDataReceived(SerialDataReceivedEventArgs args)
        {
            SerialDataReceivedEventHandler handler = (SerialDataReceivedEventHandler)Events[data_received];

            if (handler != null)
            {
                handler(this, args);
            }
        }

        [DllImport("MonoPosixHelper", SetLastError = true)]
        static extern bool poll_serial(int fd, out int error, int timeout);

        private bool Poll(Stream stream, int timeout)
        {
            CheckDisposed(stream);
            if (IsOpen == false)
            {
                throw new Exception("port is closed");
            }
            int error;

            bool poll_result = poll_serial(fd, out error, ReadTimeout);
            if (error == -1)
            {
                ThrowIOException();
            }
            return poll_result;
        }

        [DllImport("libc")]
        static extern IntPtr strerror(int errnum);

        static void ThrowIOException()
        {
            int errnum = Marshal.GetLastWin32Error();
            string error_message = Marshal.PtrToStringAnsi(strerror(errnum));

            throw new IOException(error_message);
        }

        void CheckDisposed(Stream stream)
        {
            bool disposed = (bool)disposedFieldInfo.GetValue(stream);
            if (disposed)
            {
                throw new ObjectDisposedException(stream.GetType().FullName);
            }
        }
    }

}
