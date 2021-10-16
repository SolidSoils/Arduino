using System;
using System.Threading;

using Solid.Arduino.Firmata;
using Solid.Arduino.I2C;
using System.Linq;

namespace Solid.Arduino.Run
{
    class Program
    {
        static void Main(string[] args)
        {
            // DisplayPortCapabilities();

            ISerialConnection connection = new EnhancedSerialConnection("COM4", SerialBaudRate.Bps_57600);

            using (var session = new ArduinoSession(connection))
                PerformBasicTest(session);

            Console.WriteLine("Press a key");
            Console.ReadKey(true);
        }

        private static ISerialConnection GetConnection()
        {
            Console.WriteLine("Searching Arduino connection...");
            ISerialConnection connection = EnhancedSerialConnection.Find();

            if (connection == null)
                Console.WriteLine("No connection found. Make shure your Arduino board is attached to a USB port.");
            else
                Console.WriteLine($"Connected to port {connection.PortName} at {connection.BaudRate} Baud.");

            return connection;
        }

        private static void PerformBasicTest(IFirmataProtocol session)
        {
            var firmware = session.GetFirmware();
            Console.WriteLine($"Firmware: {firmware.Name} version {firmware.MajorVersion}.{firmware.MinorVersion}");
            var protocolVersion = session.GetProtocolVersion();
            Console.WriteLine($"Firmata protocol version {protocolVersion.Major}.{protocolVersion.Minor}");

            session.SetDigitalPinMode(10, PinMode.DigitalOutput);
            session.SetDigitalPin(10, true);
            Console.WriteLine("Command sent: Light on (pin 10)");
            Console.WriteLine("Press a key");
            Console.ReadKey(true);
            session.SetDigitalPin(10, false);
            Console.WriteLine("Command sent: Light off");
        }


        private static void DisplayPortCapabilities()
        {
            using (var session = new ArduinoSession(new EnhancedSerialConnection("COM4", SerialBaudRate.Bps_57600)))
            {
                session.StringReceived += (object sender, StringEventArgs eventArgs) =>
                {
                    Console.WriteLine(eventArgs.Text);
                };

                BoardCapability cap = session.GetBoardCapability();
                Console.WriteLine();
                Console.WriteLine("Board Capability:");

                foreach (var pin in cap.Pins)
                {
                    Console.WriteLine("Pin {0}: Input: {1}, Output: {2}, Analog: {3}, Analog-Res: {4}, PWM: {5}, PWM-Res: {6}, Servo: {7}, Servo-Res: {8}, Serial: {9}, Encoder: {10}, Input-pullup: {11}",
                        pin.PinNumber,
                        pin.DigitalInput,
                        pin.DigitalOutput,
                        pin.Analog,
                        pin.AnalogResolution,
                        pin.Pwm,
                        pin.PwmResolution,
                        pin.Servo,
                        pin.ServoResolution,
                        pin.Serial,
                        pin.Encoder,
                        pin.InputPullup);
                }
            }
        }

        private void TimeTest()
        {
            var session = new ArduinoSession(new SerialConnection("COM4", SerialBaudRate.Bps_57600)) {TimeOut = 1000};
            session.MessageReceived += Session_OnMessageReceived;

            var firmata = (II2CProtocol)session;
            var x = firmata.GetI2CReply(0x68, 7);

            Console.WriteLine();
            Console.WriteLine("{0} bytes received.", x.Data.Length);
            Console.WriteLine("Starting");
            Console.WriteLine("Press a key to abort.");
            Console.ReadKey(true);

            session.Dispose();
        }

        void Session_OnMessageReceived(object sender, FirmataMessageEventArgs eventArgs)
        {
            string o;

            switch (eventArgs.Value.Type)
            {
                case MessageType.StringData:
                    o = ((StringData)eventArgs.Value.Value).Text;
                    break;

                default:
                    o = "?";
                    break;
            }

            Console.WriteLine("Message {0} received: {1}", eventArgs.Value.Type, o);
        }

        static void SimpelTest(ISerialConnection connection)
        {
            var session = new ArduinoSession(connection, timeOut: 2500);
            IFirmataProtocol firmata = session;

            firmata.AnalogStateReceived += Session_OnAnalogStateReceived;
            firmata.DigitalStateReceived += Session_OnDigitalStateReceived;

            Firmware firm = firmata.GetFirmware();
            Console.WriteLine();
            Console.WriteLine("Firmware: {0} {1}.{2}", firm.Name, firm.MajorVersion, firm.MinorVersion);
            Console.WriteLine();

            ProtocolVersion version = firmata.GetProtocolVersion();
            Console.WriteLine();
            Console.WriteLine("Protocol version: {0}.{1}", version.Major, version.Minor);
            Console.WriteLine();

            BoardCapability cap = firmata.GetBoardCapability();
            Console.WriteLine();
            Console.WriteLine("Board Capability:");

            foreach (var pin in cap.Pins)
            {
                Console.WriteLine("Pin {0}: Input: {1}, Output: {2}, Analog: {3}, Analog-Res: {4}, PWM: {5}, PWM-Res: {6}, Servo: {7}, Servo-Res: {8}, Serial: {9}, Encoder: {10}, Input-pullup: {11}",
                    pin.PinNumber,
                    pin.DigitalInput,
                    pin.DigitalOutput,
                    pin.Analog,
                    pin.AnalogResolution,
                    pin.Pwm,
                    pin.PwmResolution,
                    pin.Servo,
                    pin.ServoResolution,
                    pin.Serial,
                    pin.Encoder,
                    pin.InputPullup);
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

            foreach (var pincap in cap.Pins.Where(c => (c.DigitalInput || c.DigitalOutput) && !c.Analog))
            {
                var pinState = firmata.GetPinState(pincap.PinNumber);
                Console.WriteLine("Pin {0}: Mode = {1}, Value = {2}", pincap.PinNumber, pinState.Mode, pinState.Value);
            }
            Console.WriteLine();

            firmata.SetDigitalPort(0, 0x04);
            firmata.SetDigitalPort(1, 0xff);
            firmata.SetDigitalPinMode(10, PinMode.DigitalOutput);
            firmata.SetDigitalPinMode(11, PinMode.ServoControl);
            firmata.SetDigitalPin(11, 90);
            Thread.Sleep(500);
            int hi = 0;

            for (int a = 0; a <= 179; a += 1)
            {
                firmata.SetDigitalPin(11, a);
                Thread.Sleep(100);
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
            firmata.SetAnalogReportMode(0, true);

            Console.WriteLine("Setting digital report modes:");
            firmata.SetDigitalReportMode(0, true);
            firmata.SetDigitalReportMode(1, true);
            firmata.SetDigitalReportMode(2, true);
            Console.WriteLine();

            foreach (var pinCap in cap.Pins.Where(c => (c.DigitalInput || c.DigitalOutput) && !c.Analog))
            {
                PinState state = firmata.GetPinState(pinCap.PinNumber);
                Console.WriteLine("Digital {1} pin {0}: {2}", state.PinNumber, state.Mode, state.Value);
            }
            Console.WriteLine();

            Console.ReadLine();
            firmata.SetAnalogReportMode(0, false);
            firmata.SetDigitalReportMode(0, false);
            firmata.SetDigitalReportMode(1, false);
            firmata.SetDigitalReportMode(2, false);
            Console.WriteLine("Ready.");
        }

        static void Session_OnDigitalStateReceived(object sender, FirmataEventArgs<DigitalPortState> eventArgs)
        {
            Console.WriteLine("Digital level of port {0}: {1}", eventArgs.Value.Port, eventArgs.Value.IsSet(6) ? 'X' : 'O');
        }

        static void Session_OnAnalogStateReceived(object sender, FirmataEventArgs<AnalogState> eventArgs)
        {
            Console.WriteLine("Analog level of pin {0}: {1}", eventArgs.Value.Channel, eventArgs.Value.Level);
        }


    }
}
