using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public interface ISerialProtocol
    {
        void Write(string text);
        void Writeline(string text);
    }
}
