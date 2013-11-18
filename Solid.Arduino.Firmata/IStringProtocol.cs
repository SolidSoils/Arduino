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

        char StringTerminator { get; set; }

        void Print(string value);
        void PrintLine(string value);

        string ReadLine();
        Task<string> ReadLineAsync();

        string Read(int length = 1);
        string Read(char terminator);
        Task<string> ReadAsync(int length = 1);
        Task<string> ReadAsync(char terminator);

        string GetLine(string value);
        Task<string> GetLineAsync(string value);

        string GetString(string value, int length);
        string GetString(string value, char terminator);

        Task<string> GetStringAsync(string value, int length);
        Task<string> GetStringAsync(string value, char terminator);
    }
}
