using System;

namespace Solid.Arduino.Firmata
{
    public struct Firmware
    {
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public string Name { get; set; }
    }
}
