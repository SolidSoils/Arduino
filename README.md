# Solid.Arduino

This is a software library implementing commonly used protocols for Arduino.
Its purpose is making communication with Arduino boards in MS .NET projects easier
through a comprehensive and consistent set of methods and events.

The project's goal is to support the following protocols:

1. Serial (ASCII) messaging protocol
2. MIDI protocol
3. Firmata protocol
4. I2C protocol (as it has become part of Firmata)

Technology used: Microsoft .NET/C# v4.5

## Current status

**v0.0**

Mainly a lot of work in progress. (Pre-alpha draft)

## Milestones

1. Firmata protocol largely implemented and tested.
2. I2C protocol implemented (but not yet tested).

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

