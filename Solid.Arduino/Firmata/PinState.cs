using System;

namespace Solid.Arduino.Firmata
{
    public struct PinState
    {
        public int PinNumber { get; internal set; }
        public PinMode Mode { get; internal set; }
        public long Value { get; internal set; }
    }
}
