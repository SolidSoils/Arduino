namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents the analog level read from or set to an analog pin.
    /// </summary>
    public sealed class AnalogState
    {
        /// <summary>
        /// Gets the MIDI channel number (0 - 15).
        /// </summary>
        /// <remarks>
        /// The mapping of analog pins to channel numbers can be retrieved using the <see cref="IFirmataProtocol.GetBoardAnalogMapping"/> method.
        /// </remarks>
        public int Channel { get; internal set; }

        /// <summary>
        /// Gets the analog level.
        /// </summary>
        public long Level { get; internal set; }
    }
}
