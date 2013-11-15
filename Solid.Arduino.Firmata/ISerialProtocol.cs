using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public interface ISerialProtocol
    {
        void Print(string value);
        void PrintLine(string value);

        string Read(int length = 1);
        Task<string> ReadAsync(int length = 1);

        string ReadLine();
        Task<string> ReadLineAsync();

        string ReadString(int length);
        Task<string> ReadStringAsync(int length);

        string ReadString(char terminator);
        Task<string> ReadStringAsync(char terminator);

        string RequestLine(string value);
        Task<string> RequestLineAsync(string value);

        string RequestString(string value, int length);
        Task<string> RequestStringAsync(string value, int length);

        string RequestString(string value, char terminator);
        Task<string> RequestStringAsync(string value, char terminator);
    }
}
