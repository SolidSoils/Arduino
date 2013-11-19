using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public struct PinCapability
    {
        public int PinNumber { get; set; }
        public bool Input { get; set; }
        public bool Output { get; set; }
        public bool Analog { get; set; }
        public bool Pwm { get; set; }
        public bool Servo { get; set; }
        public int AnalogResolution { get; set; }
        public int PwmResolution { get; set; }
        public int ServoResolution { get; set; }
    }
}
