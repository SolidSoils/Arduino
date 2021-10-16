namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Contains information about the capabilities of a pin.
    /// </summary>
    public sealed class PinCapability
    {
        /// <summary>
        /// Gets the 0-based number of the pin.
        /// </summary>
        public int PinNumber { get; internal set; }

        /// <summary>
        /// Gets a value indicating if the pin can be in digital input mode.
        /// </summary>
        public bool DigitalInput { get; internal set; }

        /// <summary>
        /// Gets a value indicating if the pin can be in digital output mode.
        /// </summary>
        public bool DigitalOutput { get; internal set; }

        /// <summary>
        /// Gets a value indicating if it is an analog pin.
        /// </summary>
        public bool Analog { get; internal set; }

        /// <summary>
        /// Gets a value indicating if the pin supports pulse width modulation.
        /// </summary>
        public bool Pwm { get; internal set; }

        /// <summary>
        /// Gets a value indicating if the pin supports servo motor control.
        /// </summary>
        public bool Servo { get; internal set; }

        /// <summary>
        /// Gets the bit resolution for analog pins.
        /// </summary>
        public int AnalogResolution { get; internal set; }

        /// <summary>
        /// Gets the bit resolution for PWM enabled pins.
        /// </summary>
        public int PwmResolution { get; internal set; }

        /// <summary>
        /// Gets the bit resolution for servo enabled pins.
        /// </summary>
        public int ServoResolution { get; internal set; }

        /// <summary>
        /// Gets a value indicating if it is an I2c pin.
        /// </summary>
        public bool I2C { get; internal set; }

        /// <summary>
        /// Gets a value indicating if it is an OneWire pin.
        /// </summary>
        public bool OneWire { get; internal set; }

        /// <summary>
        /// Gets a value indicating if it is a Stepper Control pin.
        /// </summary>
        public bool StepperControl { get; internal set; }

        /// <summary>
        /// Gets a value indicating if it is an encoder pin.
        /// </summary>
        public bool Encoder { get; internal set; }

        /// <summary>
        /// Gets a value indicating if it is a serial pin.
        /// </summary>
        public bool Serial { get; internal set; }

        /// <summary>
        /// Gets a value indicating if it is an input pullup pin.
        /// </summary>
        public bool InputPullup { get; internal set; }

        /// <summary>
        /// Gets the maximum number of steps if it is a Stepper Control pin.
        /// </summary>
        public int MaxStepNumber { get; internal set; }
    }
}
