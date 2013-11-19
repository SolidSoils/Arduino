using System;

namespace Solid.Arduino.Firmata
{
    public class FirmataMessageEventArgs : EventArgs
    {

        private readonly FirmataMessage _value;

        public FirmataMessageEventArgs(FirmataMessage value)
        {
            _value = value;
        }

        public FirmataMessage Value { get { return _value; } }
    }
}
