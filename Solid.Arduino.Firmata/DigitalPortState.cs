using System;

namespace Solid.Arduino.Firmata
{
    public struct DigitalPortState
    {
        public int Port { get; set; }
        public int Pins { get; set; }

        public bool IsHigh(int pin)
        {
            if (pin < 0 || pin > 7)
                throw new ArgumentOutOfRangeException("pin", "Pin must be in range 0 - 7.");

            return (Pins & 1 << pin) > 0;
        }
    }
}
