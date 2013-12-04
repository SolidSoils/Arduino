using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino
{
    public interface ISerialConnection
    {
        event SerialDataReceivedEventHandler DataReceived;

        bool IsOpen { get; }
        string NewLine { get; set; }
        int BytesToRead { get; }

        void Open();
        void Close();
        int ReadByte();
        void Write(string text);
        void Write(byte[] buffer, int offset, int count);
        void WriteLine(string text);
    }
}
