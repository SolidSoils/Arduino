using System;

namespace Solid.Arduino.Firmata
{
    public struct PinState
    {
        public int PinNumber { get; set; }
        public PinMode Mode { get; set; }
        public ulong Value { get; set; }
    }
}
