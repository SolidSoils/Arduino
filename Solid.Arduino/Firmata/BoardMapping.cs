using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents a summary of mappings between MIDI channels and physical pin numbers.
    /// </summary>
    public struct BoardAnalogMapping
    {
        /// <summary>
        /// Gets the channel mapping array of the board's analog pins.
        /// </summary>
        public AnalogPinMapping[] PinMappings { get; internal set; }
    }
}
