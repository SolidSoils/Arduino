using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.I2c
{
    public sealed class I2cEventArgs: EventArgs
    {
        private readonly I2cReply _value;

        public I2cEventArgs(I2cReply value)
        {
            _value = value;
        }

        public I2cReply Value { get { return _value; } }

    }
}
