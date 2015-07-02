# SolidSoils4Arduino

SolidSoils4Arduino is a client library built on the .NET Framework 4.5 and providing an easy way to interact with Arduino boards.
The library implements the main communication protocols, the first of which is the Firmata protocol.
It aims to make communication with Arduino boards in MS .NET projects easier
through a comprehensive and consistent set of methods and events.

The library supports the following protocols:

1. Serial (ASCII) messaging
2. Firmata
3. I2C (as it has become part of Firmata)

The protocols mentioned can be used simultaneously. The library brokers all incoming message types
and directs them to the appropriate requestors (synchronous as well as asynchronous) and events.

Currently [Standard Firmata 2.4](https://github.com/firmata/protocol/blob/master/protocol.md) is supported.

Technology used: Microsoft .NET/C# v4.5

### Code example: Setting pin 13 HI

In this example a connection is made to an Arduino board attached to any USB port. Then pin 13 is set HI.

    using Solid.Arduino;

    (...)

	var session = new ArduinoSession(SerialConnection.FindSerialConnection());
	session.SetDigitalPin(13, true);


### Code example: Getting board capabilities

In this example the board capabilities of an Arduino device are retrieved and displayed.

    using Solid.Arduino.Firmata;

    (...)

    var connection = new SerialConnection("COM3", SerialBaudRate.Bps_57600);
	var session = new ArduinoSession(connection, timeOut: 250);
    // Cast to interface done, just for the sake of this demo.
	IFirmataProtocol firmata = (IFirmataProtocol)session;
	
	Firmware firm = firmata.GetFirmware();
	Console.WriteLine("Firmware: {0} {1}.{2}", firm.Name, firm.MajorVersion, firm.MinorVersion);
	
	ProtocolVersion version = firmata.GetProtocolVersion();
	Console.WriteLine("Protocol version: {0}.{1}", version.Major, version.Minor);
	
	BoardCapability caps = firmata.GetBoardCapability();
	Console.WriteLine("Board Capabilities:");
	
	foreach (var pincap in caps.PinCapabilities)
	{
	    Console.WriteLine("Pin {0}: Input: {1}, Output: {2}, Analog: {3}, Analog-Res: {4}, PWM: {5}, PWM-Res: {6}, Servo: {7}, Servo-Res: {8}",
	        pincap.PinNumber,
			pincap.DigitalInput,
			pincap.DigitalOutput,
			pincap.Analog,
			pincap.AnalogResolution,
			pincap.Pwm,
			pincap.PwmResolution,
	        pincap.Servo,
			pincap.ServoResolution);
	}
	Console.WriteLine();
    Console.ReadLine();

## Current status

**v0.3**

Code complete for the library core. (Beta)

## Milestones

1. Firmata protocol implemented, unit- and integration-tested.
2. I2C protocol implemented and unit-tested.
3. Serial ASCII protocol implemented and unit-tested.
4. API fully documented.
5. IObservable methods implemented (to be unittested).
6. Mono support added.

## Upcoming
1. Finish unit tests for latest additions.
2. Develop a WPF application demonstrating the library (project Solid.Arduino.Monitor).
3. Release the first version.

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
