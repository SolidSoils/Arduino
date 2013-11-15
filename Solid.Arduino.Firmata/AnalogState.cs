using System;

namespace Solid.Arduino.Firmata
{
    public struct AnalogState
    {
        public int Channel { get; set; }
        public ulong Level { get; set; }
    }
}
