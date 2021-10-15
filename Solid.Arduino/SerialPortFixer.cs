// Copyright 2010-2014 Zach Saw
// Refactored 2017 Henk van Boeijen
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Solid.Arduino
{
    /// <summary>
    /// SerialPort IOException Workaround
    /// </summary>
    /// <seealso href="http://zachsaw.blogspot.nl/2010/07/serialport-ioexception-workaround-in-c.html"/>
    internal static class SerialPortFixer
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Comstat
        {
            public readonly uint Flags;
            public readonly uint cbInQue;
            public readonly uint cbOutQue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Dcb
        {
            public readonly uint DCBlength;
            public readonly uint BaudRate;
            public uint Flags;
            public readonly ushort wReserved;
            public readonly ushort XonLim;
            public readonly ushort XoffLim;
            public readonly byte ByteSize;
            public readonly byte Parity;
            public readonly byte StopBits;
            public readonly byte XonChar;
            public readonly byte XoffChar;
            public readonly byte ErrorChar;
            public readonly byte EofChar;
            public readonly byte EvtChar;
            public readonly ushort wReserved1;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int FormatMessage(int dwFlags, HandleRef lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetCommState(SafeFileHandle hFile, ref Dcb lpDcb);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetCommState(SafeFileHandle hFile, ref Dcb lpDcb);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool ClearCommError(SafeFileHandle hFile, ref int lpErrors, ref Comstat lpStat);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, int dwShareMode, IntPtr securityAttrs, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int GetFileType(SafeFileHandle hFile);

        private const int DcbFlagAbortOnError = 14;
        private const int CommStateRetries = 10;

        public static void Initialize(string portName)
        {
            const int dwFlagsAndAttributes = 0x40000000;
            const int dwAccess = unchecked((int)0xC0000000);

            if ((portName == null) || !portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(Messages.InvalidSerialPort, nameof(portName));

            SafeFileHandle fileHandle = CreateFile(@"\\.\" + portName, dwAccess, 0, IntPtr.Zero, 3, dwFlagsAndAttributes, IntPtr.Zero);

            if (fileHandle.IsInvalid)
                ThrowIoException();

            try
            {
                int fileType = GetFileType(fileHandle);

                if ((fileType != 2) && (fileType != 0))
                    throw new ArgumentException(Messages.InvalidSerialPort, nameof(portName));

                var dcb = new Dcb();
                MarshalCommState(fileHandle, () => GetCommState(fileHandle, ref dcb));
                dcb.Flags &= ~(1u << DcbFlagAbortOnError);
                MarshalCommState(fileHandle, () => SetCommState(fileHandle, ref dcb));
            }
            finally
            {
                fileHandle.Close();
            }
        }

        private static void MarshalCommState(SafeFileHandle handle, Func<bool> performCommState)
        {
            int commErrors = 0;
            var comStat = new Comstat();

            for (int i = 0; i < CommStateRetries; i++)
            {
                if (!ClearCommError(handle, ref commErrors, ref comStat))
                    ThrowIoException();

                if (performCommState())
                    return;

                if (i == CommStateRetries - 1)
                    ThrowIoException();
            }
        }

        private static void ThrowIoException()
        {
            int errorCode = Marshal.GetLastWin32Error();
            var lpBuffer = new StringBuilder(0x200);

            string errorMessage = (FormatMessage(0x3200, new HandleRef(null, IntPtr.Zero), errorCode, 0, lpBuffer, lpBuffer.Capacity, IntPtr.Zero) != 0)
                ? lpBuffer.ToString()
                : $"0x{errorCode:X} - Unknown error";

            throw new IOException(errorMessage, (int)(0x80070000 | (uint)errorCode));
        }
    }
}