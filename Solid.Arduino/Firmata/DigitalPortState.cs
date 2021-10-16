using System;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Represents the pin states of a digital port.
    /// </summary>
    public sealed class DigitalPortState
    {
        /// <summary>
        /// Gets the digital port number.
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// Gets the bit-pattern value of the digital port.
        /// </summary>
        public int Pins { get; internal set; }

        /// <summary>
        /// Gets a value indicating if a pin is set (1 or 'high').
        /// </summary>
        /// <param name="pin">The 0-based pin number</param>
        /// <returns><c>true</c> when the pin has a binary 1 value, otherwise <c>false</c></returns>
        public bool IsSet(int pin)
        {
            if (pin < 0 || pin > 7)
                throw new ArgumentOutOfRangeException(nameof(pin), Messages.ArgumentEx_PinRange0_7);

            return (Pins & 1 << pin) > 0;
        }
    }
}
