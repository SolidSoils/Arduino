using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Solid.Arduino.I2c;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Signature of event handlers capable of processing messages.
    /// </summary>
    /// <param name="par_Sender">The object raising the event</param>
    /// <param name="par_EventArgs">Event arguments holding a <see cref="FirmataMessage"/></param>
    public delegate void MessageReceivedHandler(object par_Sender, FirmataMessageEventArgs par_EventArgs);

    /// <summary>
    /// Signature of event handlers capable of processing analog I/O messages.
    /// </summary>
    /// <param name="par_Sender">The object raising the event</param>
    /// <param name="par_EventArgs">Event arguments holding a <see cref="AnalogState"/></param>
    public delegate void AnalogStateReceivedHandler(object par_Sender, FirmataEventArgs<AnalogState> par_EventArgs);

    /// <summary>
    /// Signature of event handlers capable of processing digital I/O messages.
    /// </summary>
    /// <param name="par_Sender">The object raising the event</param>
    /// <param name="par_EventArgs">Event arguments holding a <see cref="DigitalPortState"/></param>
    public delegate void DigitalStateReceivedHandler(object par_Sender, FirmataEventArgs<DigitalPortState> par_EventArgs);

    /// <summary>
    /// The modes a pin can support or be set to.
    /// </summary>
    public enum PinMode
    {
        Undefined = -1,
        DigitalInput = 0,
        DigitalOutput = 1,
        AnalogInput = 2,
        PwmOutput = 3,
        ServoControl = 4
    }

    /// <summary>
    /// Defines a comprehensive set of members supporting the Firmata Protocol.
    /// Currently version 2.3 is supported.
    /// </summary>
    public interface IFirmataProtocol: II2cProtocol
    {
        /// <summary>
        /// Event, raised for every SysEx (0xF0) and ProtocolVersion (0xF9) message not handled by an <see cref="IFirmataProtocol"/>'s Get method.
        /// </summary>
        /// <remarks>
        /// When i.e. method <see cref="RequestBoardCapability"/> is invoked, the party system's response message raises this event.
        /// However, when method <see cref="GetBoardCapability"/> or <see cref="GetBoardCapabilityAsync"/> is invoked, the response is returned
        /// to the respective method and event <see cref="OnMessageReceived"/> is not raised.
        /// 
        /// This event is not raised for either analog or digital I/O messages.
        /// </remarks>
        event MessageReceivedHandler OnMessageReceived;

        /// <summary>
        /// Event, raised when an analog state message (command 0xE0) is received.
        /// </summary>
        /// <remarks>
        /// The frequency at which analog state messages are being sent by the party system can be set with method <see cref="SetSamplingInterval"/>.
        /// </remarks>
        event AnalogStateReceivedHandler OnAnalogStateReceived;

        /// <summary>
        /// Event, raised when a digital I/O message (command 0x90) is received.
        /// </summary>
        /// <remarks>
        /// Please note that the StandardFirmata implementation for Arduino only sends updates of digital port states if necessary.
        /// When none of a port's digital input pins have changed state since a previous polling cycle, no Firmata.sendDigitalPort message
        /// is sent.
        /// <para>
        /// Also, calling method <see cref="SetDigitalReportMode"/> does not guarantee this event will receive a (first) Firmata.sendDigitalPort message.
        /// Use method <see cref="GetPinState"/> or <see cref="GetPinStateAsync"/> inquiring the current pin states.
        /// </para>
        /// </remarks>
        event DigitalStateReceivedHandler OnDigitalStateReceived;

        /// <summary>
        /// Sends a message string.
        /// </summary>
        /// <param name="data">The message string</param>
        void SendStringData(string data);

        /// <summary>
        /// Enables or disables analog sampling reporting.
        /// </summary>
        /// <param name="channel">The channel attached to the analog pin</param>
        /// <param name="enable"><c>True</c> if enabled, otherwise <c>false</c></param>
        /// <remarks>
        /// When enabled, the party system is expected to return analog I/O messages (0xE0)
        /// for the given channel. The frequency at which these messages are returned can
        /// be controlled by method <see cref="SetSamplingInterval"/>.
        /// </remarks>
        void SetAnalogReportMode(int channel, bool enable);

        /// <summary>
        /// Sets the digital output pins of a given port LOW or HIGH.
        /// </summary>
        /// <param name="portNumber">The 0-based port number</param>
        /// <param name="pins">Binary value for the port's pins (0 to 7)</param>
        /// <remarks>
        /// A binary 1 sets the digital output pin HIGH (+5 or +3.3 volts).
        /// A binary 0 sets the digital output pin LOW.
        /// <para>
        /// The Arduino operates with 8-bit ports, so only bits 0 to 7 of the pins parameter are mapped.
        /// Higher bits are ignored.
        /// </para>
        /// <example>
        /// For port 0 bit 2 maps to the Arduino's pin 2.
        /// For port 1 bit 2 maps to pin 10.
        /// 
        /// Thet complete mapping of port 1 of the Arduino Uno looks like this:
        /// <list type="">
        /// <item>bit 0: pin 8</item>
        /// <item>bit 1: pin 9</item>
        /// <item>bit 2: pin 10</item>
        /// <item>bit 3: pin 11</item>
        /// <item>bit 4: pin 12</item>
        /// <item>bit 5: pin 13</item>
        /// <item>bit 6: not mapped</item>
        /// <item>bit 7: not mapped</item>
        /// </list> 
        /// </example>
        /// </remarks>
        void SetDigitalPortState(int portNumber, uint pins);

        /// <summary>
        /// Enables or disables digital input pin reporting for the given port.
        /// </summary>
        /// <param name="portNumber">The number of the port</param>
        /// <param name="enable"><c>true</c> if enabled, otherwise <c>false</c></param>
        /// <remarks>
        /// When enabled, the party system is expected to return digital I/O messages (0x90)
        /// for the given port.
        /// <para>
        /// Note: as for Firmata version 2.3 digital I/O messages are only returned when
        /// at least one digital input pin's state has changed from high to low or vice versa.
        /// </para>
        /// </remarks>
        void SetDigitalReportMode(int portNumber, bool enable);

        /// <summary>
        /// Sets a pin's mode (digital input/digital output/analog/PWM/servo).
        /// </summary>
        /// <param name="pinNumber">The number of the pin</param>
        /// <param name="mode">The pin's mode</param>
        void SetPinMode(int pinNumber, PinMode mode);

        /// <summary>
        /// Sets the frequency at which analog samples must be reported.
        /// </summary>
        /// <param name="milliseconds">The sampling interval in milliseconds</param>
        void SetSamplingInterval(int milliseconds);

        /// <summary>
        /// Configures the minimum and maximum pulse length for a servo pin.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <param name="minPulse">Minimum pulse length</param>
        /// <param name="maxPulse">Maximum pulse length</param>
        void ConfigureServo(int pinNumber, int minPulse, int maxPulse);

        /// <summary>
        /// Sets an analog value on a PWM or Servo enabled digital pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="value">THe position value</param>
        void SetPinValue(int pinNumber, ulong value);

        /// <summary>
        /// Sends a reset message to the party system.
        /// </summary>
        void ResetBoard();

        /// <summary>
        /// Requests the party system to send a protocol version message.
        /// </summary>
        /// <remarks>
        /// The party system is expected to return a single protocol version message (0xF9).
        /// This message triggers the <see cref="OnMessageReceived"/> event. The protocol version
        /// is passed in the <see cref="FirmataMessageEventArgs"/> in a <see cref="ProtocolVersion"/> object.
        /// </remarks>
        void RequestProtocolVersion();

        /// <summary>
        /// Gets the protocol version implemented on the party system.
        /// </summary>
        /// <returns>The implemented protocol version</returns>
        ProtocolVersion GetProtocolVersion();

        /// <summary>
        /// Asynchronously gets the protocol version implemented on the party system.
        /// </summary>
        /// <returns>The implemented protocol version</returns>
        Task<ProtocolVersion> GetProtocolVersionAsync();

        /// <summary>
        /// Requests the party system to send a firmware message.
        /// </summary>
        /// <remarks>
        /// The party system is expected to return a single SYSEX REPORT_FIRMWARE message.
        /// This message triggers the <see cref="OnMessageReceived"/> event. The firmware signature
        /// is passed in the <see cref="FirmataMessageEventArgs"/> in a <see cref="Firmware"/> object.
        /// </remarks>
        void RequestFirmware();

        /// <summary>
        /// Gets the firmware signature of the party system.
        /// </summary>
        /// <returns>The firmware signature</returns>
        Firmware GetFirmware();

        /// <summary>
        /// Asynchronously gets the firmware signature of the party system.
        /// </summary>
        /// <returns>The firmware signature</returns>
        Task<Firmware> GetFirmwareAsync();

        /// <summary>
        /// Requests the party system to send a summary of its capabilities.
        /// </summary>
        /// <remarks>
        /// The party system is expected to return a single SYSEX CAPABILITY_RESPONSE message.
        /// This message triggers the <see cref="OnMessageReceived"/> event. The capabilities
        /// are passed in the <see cref="FirmataMessageEventArgs"/> in a <see cref="BoardCapability"/> object.
        /// </remarks>
        void RequestBoardCapability();

        /// <summary>
        /// Gets a summary of the party system's capabilities.
        /// </summary>
        /// <returns>The system's capabilities</returns>
        BoardCapability GetBoardCapability();

        /// <summary>
        /// Asynchronously gets a summary of the party system's capabilities.
        /// </summary>
        /// <returns>The system's capabilities</returns>
        Task<BoardCapability> GetBoardCapabilityAsync();

        /// <summary>
        /// Requests the party system to send the channel-to-pin mappings of its analog lines.
        /// </summary>
        /// <remarks>
        /// The party system is expected to return a single SYSEX ANALOG_MAPPING_RESPONSE message.
        /// This message triggers the <see cref="OnMessageReceived"/> event. The analog mappings are
        /// passed in the <see cref="FirmataMessageEventArgs"/> in a <see cref="BoardAnalogMapping"/> object.
        /// </remarks>
        void RequestBoardAnalogMapping();

        /// <summary>
        /// Gets the channel-to-pin mappings of the party system's analog lines.
        /// </summary>
        /// <returns>The channel-to-pin mappings</returns>
        BoardAnalogMapping GetBoardAnalogMapping();

        /// <summary>
        /// Asynchronously gets the channel-to-pin mappings of the party system's analog lines.
        /// </summary>
        /// <returns>The channel-to-pin mappings</returns>
        Task<BoardAnalogMapping> GetBoardAnalogMappingAsync();

        /// <summary>
        /// Requests the party system to send the state of a given pin.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <remarks>
        /// The party system is expected to return a single SYSEX PINSTATE_RESPONSE message.
        /// This message triggers the <see cref="OnMessageReceived"/> event. The pin state
        /// is passed in the <see cref="FirmataMessageEventArgs"/> in a <see cref="PinState"/> object.
        /// </remarks>
        void RequestPinState(int pinNumber);

        /// <summary>
        /// Gets a pin's mode (digital input/output, analog etc.) and actual value.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <returns>The pin's state</returns>
        PinState GetPinState(int pinNumber);

        /// <summary>
        /// Asynchronously gets a pin's mode (digital input/output, analog etc.) and actual value.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <returns>The pin's state</returns>
        Task<PinState> GetPinStateAsync(int pinNumber);
    }
}
