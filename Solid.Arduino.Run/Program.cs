using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Solid.Arduino.Firmata;

namespace Solid.Arduino.Run
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = new FirmataSession(new SerialConnection("COM6", SerialBaudRate.Bps_57600));


            session.TimeOut = 1000;
            session.OnAnalogStateReceived += session_OnAnalogStateReceived;
            session.OnDigitalStateReceived += session_OnDigitalStateReceived;

            Firmware firm = session.GetFirmware();
            Console.WriteLine();
            Console.WriteLine("Firmware: {0} {1}.{2}", firm.Name, firm.MajorVersion, firm.MinorVersion);
            Console.WriteLine();

            var version = session.GetProtocolVersion();
            Console.WriteLine();
            Console.WriteLine("Protocol version: {0}.{1}", version.Major, version.Minor);
            Console.WriteLine();

            BoardCapability caps = session.GetBoardCapability();
            Console.WriteLine();
            Console.WriteLine("Board Capabilities:");

            foreach (var pincap in caps.PinCapabilities)
            {
                Console.WriteLine("Pin {0}: Input: {1}, Output: {2}, Analog: {3}, Analog-Res: {4}, PWM: {5}, PWM-Res: {6}, Servo: {7}, Servo-Res: {8}",
                    pincap.PinNumber, pincap.Input, pincap.Output, pincap.Analog, pincap.AnalogResolution, pincap.Pwm, pincap.PwmResolution,
                    pincap.Servo, pincap.ServoResolution);
            }
            Console.WriteLine();

            var analogMapping = session.GetBoardAnalogMapping();
            Console.WriteLine("Analog channel mappings:");

            foreach (var mapping in analogMapping.PinMappings)
            {
                Console.WriteLine("Channel {0} is mapped to pin {1}.", mapping.Channel, mapping.PinNumber);
            }
            Console.WriteLine();
            Console.WriteLine("Digital port states:");

            session.SetPinMode(5, PinMode.DigitalOutput);
            session.SetPinMode(6, PinMode.DigitalInput);
            session.SetDigitalPortState(0, 96);

            for (int x = 0; x < 20; x++)
            {
                var pinState = session.GetPinState(x);

                Console.WriteLine("Pin {0}: Mode = {1}, Value = {2}", x, pinState.Mode, pinState.Value);
            }
            Console.WriteLine();

            session.SetSamplingInterval(1000);

            //session.SetAnalogReportMode(0, false);

            session.SetDigitalReportMode(0, true);
            //session.SetDigitalReportMode(1, true);
            //session.SetDigitalReportMode(2, true);
            //session.SetDigitalReportMode(3, true);
            //session.SetDigitalReportMode(4, true);
            //session.SetDigitalReportMode(5, true);

            Console.ReadLine();
            Console.WriteLine("Ready.");
        }

        static void session_OnDigitalStateReceived(object par_Sender, FirmataEventArgs<DigitalPortState> par_EventArgs)
        {
            Console.WriteLine("Digital level of pin {0}: {1}", par_EventArgs.Value.Port, par_EventArgs.Value.Pins);
        }

        static void session_OnAnalogStateReceived(object par_Sender, FirmataEventArgs<AnalogState> par_EventArgs)
        {
            Console.WriteLine("Analog level of pin {0}: {1}", par_EventArgs.Value.Channel, par_EventArgs.Value.Level);
        }
    }
}
