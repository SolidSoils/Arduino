using System;

namespace Solid.Arduino.Firmata
{
    public struct I2cReply
    {
        public int Address { get; set; }
        public int Register { get; set; }
        public byte[] Data { get; set; }
    }
}
