using System;
using System.Runtime.CompilerServices;

namespace Solid.Arduino.Firmata
{
    public struct DigitalPortState
    {
        public int Port { get; internal set; }

        public int Pins { get; internal set; }

        public bool IsHigh(int pin)
        {
            if (pin < 0 || pin > 7)
                throw new ArgumentOutOfRangeException("pin", Messages.ArgumentEx_PinRange0_7);

            return (Pins & 1 << pin) > 0;
        }
    }
}
