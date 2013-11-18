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
            IFirmataProtocol firmata = (IFirmataProtocol)session;

            firmata.OnAnalogStateReceived += session_OnAnalogStateReceived;
            firmata.OnDigitalStateReceived += session_OnDigitalStateReceived;

            Firmware firm = firmata.GetFirmware();
            Console.WriteLine();
            Console.WriteLine("Firmware: {0} {1}.{2}", firm.Name, firm.MajorVersion, firm.MinorVersion);
            Console.WriteLine();

            var version = firmata.GetProtocolVersion();
            Console.WriteLine();
            Console.WriteLine("Protocol version: {0}.{1}", version.Major, version.Minor);
            Console.WriteLine();

            BoardCapability caps = firmata.GetBoardCapability();
            Console.WriteLine();
            Console.WriteLine("Board Capabilities:");

            foreach (var pincap in caps.PinCapabilities)
            {
                Console.WriteLine("Pin {0}: Input: {1}, Output: {2}, Analog: {3}, Analog-Res: {4}, PWM: {5}, PWM-Res: {6}, Servo: {7}, Servo-Res: {8}",
                    pincap.PinNumber, pincap.Input, pincap.Output, pincap.Analog, pincap.AnalogResolution, pincap.Pwm, pincap.PwmResolution,
                    pincap.Servo, pincap.ServoResolution);
            }
            Console.WriteLine();

            var analogMapping = firmata.GetBoardAnalogMapping();
            Console.WriteLine("Analog channel mappings:");

            foreach (var mapping in analogMapping.PinMappings)
            {
                Console.WriteLine("Channel {0} is mapped to pin {1}.", mapping.Channel, mapping.PinNumber);
            }

            firmata.ResetBoard();

            Console.WriteLine();
            Console.WriteLine("Digital port states:");

            for (int x = 0; x < 20; x++)
            {
                var pinState = firmata.GetPinState(x);

                Console.WriteLine("Pin {0}: Mode = {1}, Value = {2}", x, pinState.Mode, pinState.Value);
            }
            Console.WriteLine();

            firmata.SetDigitalPortState(0, 0x04);
            firmata.SetDigitalPortState(1, 0xff);
            firmata.SetPinMode(10, PinMode.DigitalOutput);
            firmata.SetPinMode(11, PinMode.ServoControl);
            firmata.SetPinValue(11, 90);
            firmata.ConfigureServo(11, 0, 255);
            System.Threading.Thread.Sleep(500);
            uint hi = 0;

            for (int a = 0; a <= 179; a += 1)
            {
                firmata.SetPinValue(11, (ulong)a);
                System.Threading.Thread.Sleep(100);
                firmata.SetDigitalPortState(1, hi);
                hi ^= 4;
                Console.Write("{0};", a);
            }
            Console.WriteLine();
            Console.WriteLine();

            firmata.SetPinMode(6, PinMode.DigitalInput);

            //firmata.SetDigitalPortState(2, 255);
            //firmata.SetDigitalPortState(3, 255);

            firmata.SetSamplingInterval(500);
            firmata.SetAnalogReportMode(0, false);

            firmata.SetDigitalReportMode(0, true);
            firmata.SetDigitalReportMode(1, true);
            firmata.SetDigitalReportMode(2, true);

            for (int x = 0; x < 20; x++)
            {
                PinState state = firmata.GetPinState(x);
                Console.WriteLine("Digital {1} pin {0}: {2}", x, state.Mode, state.Value);
            }
            Console.WriteLine();



            Console.ReadLine();
            firmata.SetAnalogReportMode(0, false);
            firmata.SetDigitalReportMode(0, false);
            firmata.SetDigitalReportMode(1, false);
            firmata.SetDigitalReportMode(2, false);
            Console.WriteLine("Ready.");
        }

        static void session_OnDigitalStateReceived(object par_Sender, FirmataEventArgs<DigitalPortState> par_EventArgs)
        {
            Console.WriteLine("Digital level of port {0}: {1}", par_EventArgs.Value.Port, par_EventArgs.Value.IsHigh(6) ? 'X' : 'O');
        }

        static void session_OnAnalogStateReceived(object par_Sender, FirmataEventArgs<AnalogState> par_EventArgs)
        {
            Console.WriteLine("Analog level of pin {0}: {1}", par_EventArgs.Value.Channel, par_EventArgs.Value.Level);
        }
    }
}
