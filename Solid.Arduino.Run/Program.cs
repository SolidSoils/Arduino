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
            Console.WriteLine("Started.");
            var p = new Program();
            p.TimeTest();

            Console.ReadLine();
            Console.WriteLine("Ready.");
        }

        private void TimeTest()
        {
            var session = new ArduinoSession(new SerialConnection("COM6", SerialBaudRate.Bps_57600));
            session.TimeOut = 1000;
            session.OnMessageReceived += session_OnMessageReceived;

            IFirmataProtocol firmata = (IFirmataProtocol)session;

            var x = firmata.GetI2cReply(0x68, 7);

            Console.WriteLine();
            Console.WriteLine("{0} bytes received.", x.Data.Length);

            
            Console.WriteLine("Starting");



            session.Dispose();
        }

        void session_OnMessageReceived(object par_Sender, FirmataMessageEventArgs par_EventArgs)
        {
            string o;

            switch (par_EventArgs.Value.Type)
            {
                case MessageType.StringData:
                    o = ((StringData)par_EventArgs.Value.Value).Text;
                    break;

                default:
                    o = "?";
                    break;
            }

            Console.WriteLine("Message {0} received: {1}", par_EventArgs.Value.Type, o);
        }

        static void SimpelTest()
        {
            var session = new ArduinoSession(new SerialConnection("COM6", SerialBaudRate.Bps_57600));
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

            firmata.SetDigitalPort(0, 0x04);
            firmata.SetDigitalPort(1, 0xff);
            firmata.SetDigitalPinMode(10, PinMode.DigitalOutput);
            firmata.SetDigitalPinMode(11, PinMode.ServoControl);
            firmata.SetDigitalPin(11, 90);
            firmata.ConfigureServo(11, 0, 255);
            System.Threading.Thread.Sleep(500);
            int hi = 0;

            for (int a = 0; a <= 179; a += 1)
            {
                firmata.SetDigitalPin(11, a);
                System.Threading.Thread.Sleep(100);
                firmata.SetDigitalPort(1, hi);
                hi ^= 4;
                Console.Write("{0};", a);
            }
            Console.WriteLine();
            Console.WriteLine();

            firmata.SetDigitalPinMode(6, PinMode.DigitalInput);

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
