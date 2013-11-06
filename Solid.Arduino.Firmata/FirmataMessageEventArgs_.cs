using System;

namespace Solid.Arduino.Firmata
{
    public class FirmataMessageEventArgs<T> : EventArgs
        where T : struct
    {
        private readonly T _value;

        public FirmataMessageEventArgs(T value)
        {
            _value = value;
        }

        public T Value { get { return _value; } }
    }
}
