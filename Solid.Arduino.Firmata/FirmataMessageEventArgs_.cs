using System;

namespace Solid.Arduino.Firmata
{
    public class FirmataEventArgs<T> : EventArgs
        where T : struct
    {
        private readonly T _value;

        public FirmataEventArgs(T value)
        {
            _value = value;
        }

        public T Value { get { return _value; } }
    }
}
