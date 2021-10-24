# SolidSoils4Arduino

SolidSoils4Arduino is a client library built on .NET Standard 2.1. It offers an easy way to interact with Arduino boards.
The library implements the main communication protocols, the first of which is the Firmata protocol.
It aims to make communication with Arduino boards in MS .NET projects easier
through a comprehensive and consistent set of methods and events.

The library supports the following protocols:

1. Serial (ASCII) messaging
2. Firmata
3. I2C (as it has become part of Firmata)

All protocols can be mixed. The library brokers all incoming message types
and directs them to the appropriate requestors (synchronous as well as asynchronous).

Currently [Standard Firmata 2.6](https://github.com/firmata/protocol/blob/master/protocol.md) is supported.
(Extra capabilities of Standard Firmata Plus and Configurable Firmata are currently not supported by this client library.)

Technology: C#/Microsoft .NET Standard 2.1

Dependencies: none

### Downloads

The library is available as a [NuGet package](https://www.nuget.org/packages/SolidSoils.Arduino.Client/#).

### API Documentation

See [reference documentation](https://solidsoils.github.io/Arduino/index.html).

## Getting started

#### Setup your Arduino with StandardFirmata
1. [Download the Arduino IDE](https://www.arduino.cc/en/main/software) and install it.
2. Connect your Arduino board to your computer using an USB cable.
3. Start the Arduino IDE and navigate to File > Examples > Firmata > StandardFirmata.
4. Upload the sketch.

#### Basic test (C#)
Preparation
- Your Arduino is setup with the StandardFirmata sketch (see above).
- An LED is connected to pin 10 of your Arduino.

Further steps
1. Open Visual Studio and create a new C# console program project.
2. Add NuGet package [SolidSoils.Arduino.Client](https://www.nuget.org/packages/SolidSoils.Arduino.Client/).
3. In Program.cs put the following code:

```csharp
using System;
using Solid.Arduino.Firmata;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            ISerialConnection connection = GetConnection();

            if (connection != null)
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
                Console.WriteLine($"Connected to port {connection.PortName} at {connection.BaudRate} baud.");

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
    }
}
```

#### Display board capabilities
Preparation
- Your Arduino is setup with the StandardFirmata sketch (see above).
- In this example the Arduino is connected to COM3 at 57600 baud. Modify as needed.

Further steps
1. Open Visual Studio and create a new C# console program project.
2. Add NuGet package [SolidSoils.Arduino.Client](https://www.nuget.org/packages/SolidSoils.Arduino.Client/).
3. In Program.cs put the following code:

```csharp
using System;
using Solid.Arduino.Firmata;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            DisplayPortCapabilities();
            Console.WriteLine("Press a key");
            Console.ReadKey(true);
        }

        private static void DisplayPortCapabilities()
        {
            using (var session = new ArduinoSession(new EnhancedSerialConnection("COM3", SerialBaudRate.Bps_57600)))
            {
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
    }
}
```

## Current status

**v1.0.0**

## Milestones

1. Firmata protocol implemented, unit- and integration-tested.
2. I2C protocol implemented and unit-tested.
3. Serial ASCII protocol implemented and unit-tested.
4. API fully documented.
5. IObservable methods implemented (to be unittested).
6. Mono support added.
7. NuGet package published.

## License
[BSD-2 license](https://github.com/SolidSoils/Arduino/blob/master/LICENSE.md)

## Contributing
If you discover a bug or would like to propose a new feature,
please open a new [issue](https://github.com/solidsoils/arduino/issues?sort=created&state=open).

To contribute, fork this respository and create a new topic branch for the bug,
feature or other existing issue you are addressing. Submit the pull request against the *master* branch.

If you would like to contribute but don't have a specific bugfix or new feature to contribute,
you can take on an existing issue. Add a comment to
the issue to express your intent to begin work and/or to get any additional information about the issue.

Please, test your contributed code thoroughly. In your pull request, describe tests performed to ensure 
that no existing code is broken and that any changes maintain backwards compatibility with the existing API.
