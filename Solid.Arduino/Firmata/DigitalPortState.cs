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
                throw new ArgumentOutOfRangeException("pin", Messages.ArgumentEx_PinRange0_7);

            return (Pins & 1 << pin) > 0;
        }
    }
}
