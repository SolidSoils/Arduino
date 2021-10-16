namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Identifies the Arduino board's firmware.
    /// </summary>
    public sealed class Firmware
    {
        /// <summary>
        /// Gets the major version number.
        /// </summary>
        public int MajorVersion { get; internal set; }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        public int MinorVersion { get; internal set; }

        /// <summary>
        /// Gets the name of the board's firmware.
        /// </summary>
        public string Name { get; internal set; }
    }
}
