namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Contains information about a pin's state.
    /// </summary>
    public sealed class PinState
    {
        /// <summary>
        /// The 0-based pin number
        /// </summary>
        public int PinNumber { get; internal set; }

        /// <summary>
        /// Gets pin's operating mode.
        /// </summary>
        public PinMode Mode { get; internal set; }

        /// <summary>
        /// Gets the value of the pin.
        /// </summary>
        /// <remarks>
        /// For analog pins the value is 0 or a positive number. For digital pins a low is represented by 0 and a high is respresented by 1.
        /// </remarks>
        public long Value { get; internal set; }
    }
}
