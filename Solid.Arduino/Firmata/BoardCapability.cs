namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents a summary of pinmode capabilities supported by an Arduino board.
    /// </summary>
    public struct BoardCapability
    {
        /// <summary>
        /// Gets the capability array of the board's pins.
        /// </summary>
        public PinCapability[] Pins { get; internal set; }
    }
}
