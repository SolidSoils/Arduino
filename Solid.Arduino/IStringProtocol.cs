using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino
{
    public delegate void StringReceivedHandler(object par_Sender, StringEventArgs par_EventArgs);

    public interface IStringProtocol
    {
        event StringReceivedHandler StringReceived;

        string NewLine { get; set; }

        void Write(string value = null);
        void WriteLine(string value = null);

        string ReadLine();
        Task<string> ReadLineAsync();

        string Read(int length = 1);
        Task<string> ReadAsync(int length = 1);

        string ReadTo(char terminator);
        Task<string> ReadToAsync(char terminator);
    }
}
