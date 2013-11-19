using System;

namespace Solid.Arduino.I2c
{
    public struct I2cReply
    {
        public int Address { get; set; }
        public int Register { get; set; }
        public byte[] Data { get; set; }
    }
}
