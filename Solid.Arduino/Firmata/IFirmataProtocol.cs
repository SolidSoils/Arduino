using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    /// <summary>
    /// Signature of event handlers capable of processing Firmata messages.
    /// </summary>
    /// <param name="sender">The object raising the event</param>
    /// <param name="eventArgs">Event arguments holding a <see cref="FirmataMessage"/></param>
    public delegate void MessageReceivedHandler(object sender, FirmataMessageEventArgs eventArgs);

    /// <summary>
    /// Signature of event handlers capable of processing analog I/O messages.
    /// </summary>
    /// <param name="sender">The object raising the event</param>
    /// <param name="eventArgs">Event arguments holding a <see cref="AnalogState"/></param>
    public delegate void AnalogStateReceivedHandler(object sender, FirmataEventArgs<AnalogState> eventArgs);

    /// <summary>
    /// Signature of event handlers capable of processing digital I/O messages.
    /// </summary>
    /// <param name="sender">The object raising the event</param>
    /// <param name="eventArgs">Event arguments holding a <see cref="DigitalPortState"/></param>
    public delegate void DigitalStateReceivedHandler(object sender, FirmataEventArgs<DigitalPortState> eventArgs);

    /// <summary>
    /// The modes a pin can be in or can be set to.
    /// </summary>
    public enum PinMode
    {
        Undefined = -1,
        DigitalInput = 0,
        DigitalOutput = 1,
        AnalogInput = 2,
        PwmOutput = 3,
        ServoControl = 4,
        I2C = 6,
        OneWire = 7,
        StepperControl = 8,
        Encoder = 9
    }

    /// <summary>
    /// Defines a comprehensive set of members supporting the Firmata Protocol.
    /// Currently version 2.3 is supported.
    /// </summary>
    /// <seealso href="https://github.com/firmata/arduino">Firmata project on GitHub</seealso>
    /// <seealso href="https://github.com/firmata/protocol">Firmata protocol details</seealso>
    /// <seealso href="http://arduino.cc/en/reference/firmata">Firmata reference for Arduino</seealso>
    public interface IFirmataProtocol
    {
        /// <summary>
        /// Event, raised for every SysEx (0xF0) and ProtocolVersion (0xF9) message not handled by an <see cref="IFirmataProtocol"/>'s Get method.
        /// </summary>
        /// <remarks>
        /// When e.g. method <see cref="RequestBoardCapability"/> is invoked, the party system's response message raises this event.
        /// However, when method <see cref="GetBoardCapability"/> or <see cref="GetBoardCapabilityAsync"/> is invoked, the response is returned
        /// to the respective method and event <see cref="MessageReceived"/> is not raised.
        /// 
        /// This event is not raised for either analog or digital I/O messages.
        /// </remarks>
        event MessageReceivedHandler MessageReceived;

        /// <summary>
        /// Event, raised when an analog state message (command 0xE0) is received.
        /// </summary>
        /// <remarks>
        /// The frequency at which analog state messages are being sent by the party system can be set with method <see cref="SetSamplingInterval"/>.
        /// </remarks>
        event AnalogStateReceivedHandler AnalogStateReceived;

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
        event DigitalStateReceivedHandler DigitalStateReceived;

        /// <summary>
        /// Creates an observable object tracking <see cref="AnalogState"/> messages.
        /// </summary>
        /// <returns>An <see cref="IObservable{AnalogState}"/> interface</returns>
        IObservable<AnalogState> CreateAnalogStateMonitor();

        /// <summary>
        /// Creates an observable object tracking <see cref="AnalogState" /> messages for a specific channel.
        /// </summary>
        /// <param name="channel">The channel to track</param>
        /// <returns>
        /// An <see cref="IObservable{AnalogState}" /> interface
        /// </returns>
        IObservable<AnalogState> CreateAnalogStateMonitor(int channel);

        /// <summary>
        /// Creates an observable object tracking <see cref="DigitalPortState"/> messages.
        /// </summary>
        /// <returns>An <see cref="IObservable{DigitalPortState}"/> interface</returns>
        IObservable<DigitalPortState> CreateDigitalStateMonitor();

        /// <summary>
        /// Creates an observable object tracking <see cref="DigitalPortState" /> messages for a specific port.
        /// </summary>
        /// <param name="port">The port to track</param>
        /// <returns>
        /// An <see cref="IObservable{DigitalPortState}" /> interface
        /// </returns>
        IObservable<DigitalPortState> CreateDigitalStateMonitor(int port);

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
        /// For port 0 bit 2 maps to the Arduino Uno's pin 2.
        /// For port 1 bit 2 maps to pin 10.
        /// 
        /// The complete mapping of port 1 of the Arduino Uno looks like this:
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
        void SetDigitalPort(int portNumber, int pins);

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
        /// Sets a pin's mode (digital input/digital output/analog/PWM/servo etc.).
        /// </summary>
        /// <param name="pinNumber">The number of the pin</param>
        /// <param name="mode">The pin's mode</param>
        void SetDigitalPinMode(int pinNumber, PinMode mode);

        /// <summary>
        /// Sets the frequency at which analog samples must be reported.
        /// </summary>
        /// <param name="milliseconds">The sampling interval in milliseconds</param>
        void SetSamplingInterval(int milliseconds);

        /// <summary>
        /// Sets an analog value on a PWM or Servo enabled analog output pin.
        /// </summary>
        /// <param name="pinNumber">The pin number.</param>
        /// <param name="value">The value</param>
        void SetDigitalPin(int pinNumber, long value);

        /// <summary>
        /// Sets a HI or LO value on a digital output pin.
        /// </summary>
        /// <param name="pinNumber">The pin number</param>
        /// <param name="value">The value (<c>false</c> = Low, <c>true</c> = High)</param>
        void SetDigitalPin(int pinNumber, bool value);

        /// <summary>
        /// Sends a reset message to the party system.
        /// </summary>
        void ResetBoard();

        /// <summary>
        /// Requests the party system to send a protocol version message.
        /// </summary>
        /// <remarks>
        /// The party system is expected to return a single protocol version message (0xF9).
        /// This message triggers the <see cref="MessageReceived"/> event. The protocol version
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
        /// This message triggers the <see cref="MessageReceived"/> event. The firmware signature
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
        /// This message triggers the <see cref="MessageReceived"/> event. The capabilities
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
        /// This message triggers the <see cref="MessageReceived"/> event. The analog mappings are
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
        /// This message triggers the <see cref="MessageReceived"/> event. The pin state
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
