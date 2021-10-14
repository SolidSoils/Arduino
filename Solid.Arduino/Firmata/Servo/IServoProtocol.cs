namespace Solid.Arduino.Firmata.Servo
{
    /// <summary>
    /// Defines Servo control related members of the Firmata protocol.
    /// </summary>
    /// <remarks>
    /// This interface is separated from the <see cref="IFirmataProtocol"/> interface, in order to
    /// protect the latter against feature bloat. 
    /// </remarks>
    public interface IServoProtocol
    {
        /// <summary>
        /// Configures the minimum and maximum pulse length for a servo pin.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <param name="minPulse">Minimum pulse length</param>
        /// <param name="maxPulse">Maximum pulse length</param>
        void ConfigureServo(int pinNumber, int minPulse, int maxPulse);
    }
}
