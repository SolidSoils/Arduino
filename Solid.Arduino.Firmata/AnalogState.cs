using System;

namespace Solid.Arduino.Firmata
{
    public struct AnalogState
    {
        public int Pin { get; set; }
        public ulong Level { get; set; }
    }
}
