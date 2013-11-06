using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public struct BoardCapability
    {
        public PinCapability[] PinCapabilities { get; set; }
    }
}
