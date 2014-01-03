# Solid.Arduino

Solid.Arduino is a client library targeting .NET 4.5 and above that provides an easy way to interact with the Arduino.
The library implements a few communication protocols, the first of which is the Firmata protocol.
It aims to make communication with Arduino boards in MS .NET projects easier
through a comprehensive and consistent set of methods and events.

The library supports the following protocols:

1. Serial (ASCII) messaging protocol
2. Firmata protocol
3. I2C protocol (as it has become part of Firmata)

The protocols mentioned can be used simultaneously. The library brokers all incoming message types
and directs them to the proper requestors (synchronous as well as asynchronous) and events.

Technology used: Microsoft .NET/C# v4.5

## Current status

**v0.5**

Code complete for the library core. (Pre-beta)

## Milestones

1. Firmata protocol implemented, unit- and integration-tested.
2. I2C protocol implemented.
3. Serial ASCII protocol implemented.
4. Interfaces documented.
5. IObservable methods implemented.

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

