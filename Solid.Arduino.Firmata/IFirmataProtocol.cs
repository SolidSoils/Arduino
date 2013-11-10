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
        event DigitalStateReceivedHandler OnDigitalStateReceived;

        void SendStringData(string data);
        void SendProtocolVersion(int majorVersion, int minorVersion);
        void SetAnalogLevel(int channel, ulong level);
        void SetAnalogReportMode(int channel, bool enable);
        void SetDigitalPortState(int portNumber, uint pins);
        void SetDigitalReportMode(int portNumber, bool enable);
        void SetPinMode(int pinNumber, PinMode mode);
        void SetSamplingInterval(int milliseconds);
        void ConfigureServo(int pinNumber, int minPulse, int maxPulse);
        void ResetBoard();

        void RequestFirmware();
        Firmware GetFirmware();
        Task<Firmware> GetFirmwareAsync();

        void RequestBoardCapability();
        BoardCapability GetBoardCapability();
        Task<BoardCapability> GetBoardCapabilityAsync();

        void RequestBoardAnalogMapping();
        BoardAnalogMapping GetBoardAnalogMapping();
        Task<BoardAnalogMapping> GetBoardAnalogMappingAsync();

        void RequestDigitalPortState(int pinNumber);
        DigitalPortState GetDigitalPortState(int pinNumber);
        Task<DigitalPortState> GetDigitalPortStateAsync(int pinNumber);
    }
}
