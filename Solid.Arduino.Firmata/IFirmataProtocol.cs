using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino.Firmata
{
    public delegate void MessageReceivedHandler(object par_Sender, FirmataMessageEventArgs par_EventArgs);
    public delegate void AnalogStateReceivedHandler(object par_Sender, FirmataEventArgs<AnalogState> par_EventArgs);
    public delegate void DigitalStateReceivedHandler(object par_Sender, FirmataEventArgs<DigitalPortState> par_EventArgs);

    public enum PinMode
    {
        None = -1,
        Input = 0,
        Output = 1,
        Analog = 2,
        Pwm = 3,
        Servo = 4
    }

    public interface IFirmataProtocol: II2cProtocol
    {
        event MessageReceivedHandler OnMessageReceived;
        event AnalogStateReceivedHandler OnAnalogStateReceived;

        /// <summary>
        /// Event handler, raised when a digital I/O message (command 0x90) is received.
        /// </summary>
        /// <remarks>
        /// Please note that the StandardFirmata implementation for Arduino only sends updates of digital port states if necessary.
        /// When none of a port's pins have changed state since a previous polling cycle, no Firmata.sendDigitalPort message
        /// is sent.
        /// <para>
        /// Also, calling method <see cref="SetDigitalReportMode"/> does not guarantee this event will receive a (first) Firmata.sendDigitalPort message.
        /// Use method <see cref="GetPinState"/> or <see cref="GetPinStateAsync"/> inquiring the current pin states.
        /// </para>
        /// </remarks>
        event DigitalStateReceivedHandler OnDigitalStateReceived;

        void SendStringData(string data);
        void SetAnalogLevel(int channel, ulong level);
        void SetAnalogReportMode(int channel, bool enable);
        void SetDigitalPortState(int portNumber, uint pins);
        void SetDigitalReportMode(int portNumber, bool enable);
        void SetPinMode(int pinNumber, PinMode mode);
        void SetSamplingInterval(int milliseconds);
        void ConfigureServo(int pinNumber, int minPulse, int maxPulse);
        void ResetBoard();

        void RequestProtocolVersion();
        ProtocolVersion GetProtocolVersion();
        Task<ProtocolVersion> GetProtocolVersionAsync();

        void RequestFirmware();
        Firmware GetFirmware();
        Task<Firmware> GetFirmwareAsync();

        void RequestBoardCapability();
        BoardCapability GetBoardCapability();
        Task<BoardCapability> GetBoardCapabilityAsync();

        void RequestBoardAnalogMapping();
        BoardAnalogMapping GetBoardAnalogMapping();
        Task<BoardAnalogMapping> GetBoardAnalogMappingAsync();

        void RequestPinState(int pinNumber);
        PinState GetPinState(int pinNumber);
        Task<PinState> GetPinStateAsync(int pinNumber);
    }
}
