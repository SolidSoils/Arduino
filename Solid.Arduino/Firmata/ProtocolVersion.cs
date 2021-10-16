namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents the Firmata communication protocol version.
    /// </summary>
    public sealed class ProtocolVersion
    {
        /// <summary>
        /// Gets or sets the major version number.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Gets or sets the minor version number.
        /// </summary>
        public int Minor { get; set; }
    }
}
