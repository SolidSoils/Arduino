using System;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents a mapping between a MIDI channel and a physical pin number.
    /// </summary>
    public struct AnalogPinMapping
    {
        /// <summary>
        /// Gets the MIDI channel number (0 - 15).
        /// </summary>
        public int Channel { get; internal set; }

        /// <summary>
        /// Gets the board's pin number (0 - 127).
        /// </summary>
        public int PinNumber { get; internal set; }
    }
}
