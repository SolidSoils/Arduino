namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents a summary of pinmode capabilities supported by an Arduino board.
    /// </summary>
    public sealed class BoardCapability
    {
        /// <summary>
        /// Gets the capability array of the board's pins.
        /// </summary>
        public PinCapability[] Pins { get; internal set; }
    }
}
