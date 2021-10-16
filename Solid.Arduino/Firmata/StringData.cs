namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents a string exchanged with the Firmata SYSEX STRING_DATA command.
    /// </summary>
    public sealed class StringData
    {
        /// <summary>
        /// Gets or sets the string.
        /// </summary>
        public string Text { get; set; }
    }
}
