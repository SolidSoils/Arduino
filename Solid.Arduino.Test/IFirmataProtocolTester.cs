using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino.Firmata;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class IFirmataProtocolTester
    {
        private readonly Queue<FirmataMessage> _messagesReceived = new Queue<FirmataMessage>();

        [TestMethod]
        public void ResetBoard()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xFF }, new byte[] { 0xF9, 2, 3 });
            session.ResetBoard();

            Assert.AreEqual(1, _messagesReceived.Count);
            FirmataMessage message = _messagesReceived.Dequeue();
            Assert.AreEqual(MessageType.ProtocolVersion, message.Type);
            var version = (ProtocolVersion)message.Value;
            Assert.AreEqual(2, version.Major);
            Assert.AreEqual(3, version.Minor);
        }

        [TestMethod]
        public void GetProtocolVersion()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF9 }, new byte[] { 0xF9, 2, 3 });

            ProtocolVersion version = session.GetProtocolVersion();
            Assert.AreEqual(2, version.Major);
            Assert.AreEqual(3, version.Minor);
        }

        [TestMethod]
        public void GetProtocolVersionAsync()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF9 }, new byte[] { 0xF9, 2, 3 });
            ProtocolVersion version = session.GetProtocolVersionAsync().Result;
            Assert.AreEqual(2, version.Major);
            Assert.AreEqual(3, version.Minor);
            Assert.AreEqual(1, _messagesReceived.Count);
        }


        [TestMethod]
        public void RequestProtocolVersion()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF9 }, new byte[] { 0xF9, 2, 3 });

            session.RequestProtocolVersion();

            Assert.AreEqual(1, _messagesReceived.Count, "Message event error");
            FirmataMessage message = _messagesReceived.Dequeue();
            Assert.AreEqual(MessageType.ProtocolVersion, message.Type);
            var version = (ProtocolVersion)message.Value;
            Assert.AreEqual(2, version.Major);
            Assert.AreEqual(3, version.Minor);
        }

        [TestMethod]
        public void GetFirmware()
        {
            const int majorVersion = 3;
            const int minorVersion = 7;
            const string Name = "Arduino Firmata";

            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x79, 0xF7 }, new byte[] { 0xF0, 0x79, majorVersion, minorVersion });
            connection.EnqueueResponse(Name.To14BitIso());
            connection.EnqueueResponse(0xF7);

            Firmware firmware = session.GetFirmware();
            Assert.AreEqual(firmware.MajorVersion, majorVersion);
            Assert.AreEqual(firmware.MinorVersion, minorVersion);
            Assert.AreEqual(firmware.Name, Name);
        }

        
        [TestMethod]
        public void GetFirmwareAsync()
        {
            const int majorVersion = 5;
            const int minorVersion = 1;
            const string Name = "Arduïno Firmata";

            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x79, 0xF7 }, new byte[] { 0xF0, 0x79, majorVersion, minorVersion });
            connection.EnqueueResponse(Name.To14BitIso());
            connection.EnqueueResponse(0xF7);

            Firmware firmware = session.GetFirmwareAsync().Result;
            Assert.AreEqual(firmware.MajorVersion, majorVersion);
            Assert.AreEqual(firmware.MinorVersion, minorVersion);
            Assert.AreEqual(firmware.Name, Name);
        }

        [TestMethod]
        public void RequestFirmware()
        {
            const int majorVersion = 5;
            const int minorVersion = 1;
            const string Name = "Arduïno Firmata";

            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x79, 0xF7 }, new byte[] { 0xF0, 0x79, majorVersion, minorVersion });
            connection.EnqueueResponse(Name.To14BitIso());
            connection.EnqueueResponse(0xF7);

            session.RequestFirmware();
            Assert.AreEqual(1, _messagesReceived.Count, "Message event error");
            FirmataMessage message = _messagesReceived.Dequeue();
            Assert.AreEqual(MessageType.FirmwareResponse, message.Type);
            var firmware = (Firmware)message.Value;

            Assert.AreEqual(firmware.MajorVersion, majorVersion);
            Assert.AreEqual(firmware.MinorVersion, minorVersion);
            Assert.AreEqual(firmware.Name, Name);
        }        

        [TestMethod]
        public void GetBoardCapability()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            // DIGITAL INPUT/DIGITAL OUTPUT/ANALOG/PWM/SERVO/I2C, 0/1/2/3/4/6
            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6B, 0xF7 }, 0xF0, 0x6C);
            connection.EnqueueResponse(0, 1, 1, 1, 3, 10, 6, 1, 0x7F);
            connection.EnqueueResponse(0xF7);

            BoardCapability capability = session.GetBoardCapability();
            Assert.AreEqual(1, capability.Pins.Length);

            Assert.AreEqual(0, capability.Pins[0].PinNumber);
            Assert.AreEqual(true, capability.Pins[0].DigitalInput);
            Assert.AreEqual(true, capability.Pins[0].DigitalOutput);
            Assert.AreEqual(true, capability.Pins[0].Pwm);
            Assert.AreEqual(10, capability.Pins[0].PwmResolution);
            Assert.AreEqual(true, capability.Pins[0].I2C);

            Assert.AreEqual(false, capability.Pins[0].Analog);
            Assert.AreEqual(0, capability.Pins[0].AnalogResolution);
            Assert.AreEqual(false, capability.Pins[0].Servo);
            Assert.AreEqual(0, capability.Pins[0].ServoResolution);
        }


        [TestMethod]
        public void GetBoardCapabilityAsync()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            // DIGITAL INPUT/DIGITAL OUTPUT/ANALOG/PWM/SERVO, 0/1/2/3/4
            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6B, 0xF7 }, 0xF0, 0x6C);
            connection.EnqueueResponse(2, 8, 0x7F);
            connection.EnqueueResponse(0, 1, 1, 1, 0x7F);
            connection.EnqueueResponse(1, 1, 3, 7, 4, 7, 0x7F);
            connection.EnqueueResponse(0xF7);

            BoardCapability capability = session.GetBoardCapabilityAsync().Result;
            Assert.AreEqual(3, capability.Pins.Length);

            Assert.AreEqual(0, capability.Pins[0].PinNumber);
            Assert.AreEqual(false, capability.Pins[0].DigitalInput);
            Assert.AreEqual(false, capability.Pins[0].DigitalOutput);
            Assert.AreEqual(true, capability.Pins[0].Analog);
            Assert.AreEqual(8, capability.Pins[0].AnalogResolution);
            Assert.AreEqual(false, capability.Pins[0].Pwm);
            Assert.AreEqual(0, capability.Pins[0].PwmResolution);
            Assert.AreEqual(false, capability.Pins[0].Servo);
            Assert.AreEqual(0, capability.Pins[0].ServoResolution);

            Assert.AreEqual(1, capability.Pins[1].PinNumber);
            Assert.AreEqual(true, capability.Pins[1].DigitalInput);
            Assert.AreEqual(true, capability.Pins[1].DigitalOutput);
            Assert.AreEqual(false, capability.Pins[1].Analog);
            Assert.AreEqual(0, capability.Pins[1].AnalogResolution);
            Assert.AreEqual(false, capability.Pins[1].Pwm);
            Assert.AreEqual(0, capability.Pins[1].PwmResolution);
            Assert.AreEqual(false, capability.Pins[1].Servo);
            Assert.AreEqual(0, capability.Pins[1].ServoResolution);

            Assert.AreEqual(2, capability.Pins[2].PinNumber);
            Assert.AreEqual(false, capability.Pins[2].DigitalInput);
            Assert.AreEqual(true, capability.Pins[2].DigitalOutput);
            Assert.AreEqual(false, capability.Pins[2].Analog);
            Assert.AreEqual(0, capability.Pins[2].AnalogResolution);
            Assert.AreEqual(true, capability.Pins[2].Pwm);
            Assert.AreEqual(7, capability.Pins[2].PwmResolution);
            Assert.AreEqual(true, capability.Pins[2].Servo);
            Assert.AreEqual(7, capability.Pins[2].ServoResolution);
        }

        [TestMethod]
        public void RequestBoardCapability()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            // DIGITAL INPUT/DIGITAL OUTPUT/ANALOG/PWM/SERVO, 0/1/2/3/4
            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6B, 0xF7 }, 0xF0, 0x6C);
            connection.EnqueueResponse(2, 8, 0x7F);
            connection.EnqueueResponse(0, 127, 1, 127, 0x7F);
            connection.EnqueueResponse(0xF7);

            session.RequestBoardCapability();
            FirmataMessage message = _messagesReceived.Dequeue();

            Assert.AreEqual(MessageType.CapabilityResponse, message.Type);
            BoardCapability capability = (BoardCapability)message.Value;

            Assert.AreEqual(2, capability.Pins.Length);
            Assert.AreEqual(0, capability.Pins[0].PinNumber);
            Assert.AreEqual(false, capability.Pins[0].DigitalInput);
            Assert.AreEqual(false, capability.Pins[0].DigitalOutput);
            Assert.AreEqual(true, capability.Pins[0].Analog);
            Assert.AreEqual(8, capability.Pins[0].AnalogResolution);
            Assert.AreEqual(false, capability.Pins[0].Pwm);
            Assert.AreEqual(0, capability.Pins[0].PwmResolution);
            Assert.AreEqual(false, capability.Pins[0].Servo);
            Assert.AreEqual(0, capability.Pins[0].ServoResolution);

            Assert.AreEqual(1, capability.Pins[1].PinNumber);
            Assert.AreEqual(true, capability.Pins[1].DigitalInput);
            Assert.AreEqual(true, capability.Pins[1].DigitalOutput);
            Assert.AreEqual(false, capability.Pins[1].Analog);
            Assert.AreEqual(0, capability.Pins[1].AnalogResolution);
            Assert.AreEqual(false, capability.Pins[1].Pwm);
            Assert.AreEqual(0, capability.Pins[1].PwmResolution);
            Assert.AreEqual(false, capability.Pins[1].Servo);
            Assert.AreEqual(0, capability.Pins[1].ServoResolution);
        }

        [TestMethod]
        public void GetBoardAnalogMapping()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x69, 0xF7 }, 0xF0, 0x6A);
            connection.EnqueueResponse(0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x00, 0x01, 0x02);
            connection.EnqueueResponse(0x03, 0x04, 0x05, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F);
            connection.EnqueueResponse(0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x7F, 0x0F);
            connection.EnqueueResponse(0xF7);

            BoardAnalogMapping mapping = session.GetBoardAnalogMapping();

            Assert.AreEqual(7, mapping.PinMappings.Length);
            Assert.AreEqual(5, mapping.PinMappings[0].PinNumber);
            Assert.AreEqual(0, mapping.PinMappings[0].Channel);
            Assert.AreEqual(6, mapping.PinMappings[1].PinNumber);
            Assert.AreEqual(1, mapping.PinMappings[1].Channel);
            Assert.AreEqual(7, mapping.PinMappings[2].PinNumber);
            Assert.AreEqual(2, mapping.PinMappings[2].Channel);
            Assert.AreEqual(8, mapping.PinMappings[3].PinNumber);
            Assert.AreEqual(3, mapping.PinMappings[3].Channel);
            Assert.AreEqual(9, mapping.PinMappings[4].PinNumber);
            Assert.AreEqual(4, mapping.PinMappings[4].Channel);
            Assert.AreEqual(10, mapping.PinMappings[5].PinNumber);
            Assert.AreEqual(5, mapping.PinMappings[5].Channel);
            Assert.AreEqual(23, mapping.PinMappings[6].PinNumber);
            Assert.AreEqual(15, mapping.PinMappings[6].Channel);
        }

        [TestMethod]
        public void GetBoardAnalogMappingAsync()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x69, 0xF7 }, 0xF0, 0x6A);
            connection.EnqueueResponse(0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x7F, 0x7F);
            connection.EnqueueResponse(0xF7);

            BoardAnalogMapping mapping = session.GetBoardAnalogMappingAsync().Result;

            Assert.AreEqual(6, mapping.PinMappings.Length);
            Assert.AreEqual(0, mapping.PinMappings[0].PinNumber);
            Assert.AreEqual(0, mapping.PinMappings[0].Channel);
            Assert.AreEqual(1, mapping.PinMappings[1].PinNumber);
            Assert.AreEqual(1, mapping.PinMappings[1].Channel);
            Assert.AreEqual(2, mapping.PinMappings[2].PinNumber);
            Assert.AreEqual(2, mapping.PinMappings[2].Channel);
            Assert.AreEqual(3, mapping.PinMappings[3].PinNumber);
            Assert.AreEqual(3, mapping.PinMappings[3].Channel);
            Assert.AreEqual(4, mapping.PinMappings[4].PinNumber);
            Assert.AreEqual(4, mapping.PinMappings[4].Channel);
            Assert.AreEqual(5, mapping.PinMappings[5].PinNumber);
            Assert.AreEqual(5, mapping.PinMappings[5].Channel);
        }

        [TestMethod]
        public void RequestBoardAnalogMapping()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x69, 0xF7 }, 0xF0, 0x6A);
            connection.EnqueueResponse(0x0F);
            connection.EnqueueResponse(0xF7);

            session.RequestBoardAnalogMapping();
            FirmataMessage message = _messagesReceived.Dequeue();
            Assert.AreEqual(MessageType.AnalogMappingResponse, message.Type);

            BoardAnalogMapping mapping = (BoardAnalogMapping)message.Value;

            Assert.AreEqual(1, mapping.PinMappings.Length);
            Assert.AreEqual(0, mapping.PinMappings[0].PinNumber);
            Assert.AreEqual(15, mapping.PinMappings[0].Channel);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetPinState_NegativePinNumber_Argument()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6D, 0x00, 0xF7 }, 0xF0, 0x6E);
            // Pin 0 analog mode, value = 1023
            connection.EnqueueResponse(0x00, 0x02, 0x7F, 0x07);
            connection.EnqueueResponse(0xF7);

            PinState state = session.GetPinState(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetPinState_PinNumber_Argument_Is128()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6D, 0x00, 0xF7 }, 0xF0, 0x6E);
            // Pin 0 analog mode, value = 1023
            connection.EnqueueResponse(0x00, 0x02, 0x7F, 0x07);
            connection.EnqueueResponse(0xF7);

            PinState state = session.GetPinState(128);
        }

        [TestMethod]
        public void GetPinState_PinNumber_Argument_Is0()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6D, 0x00, 0xF7 }, 0xF0, 0x6E);
            // Pin 0 analog mode, value = 1023
            connection.EnqueueResponse(0x00, 0x02, 0x7F, 0x07);
            connection.EnqueueResponse(0xF7);

            PinState state = session.GetPinState(0);

            Assert.AreEqual(0, state.PinNumber);
            Assert.AreEqual(PinMode.AnalogInput, state.Mode);
            Assert.AreEqual(1023U, state.Value);
        }

        [TestMethod]
        public void GetPinState_PinNumber_Argument_Is127_Returned_Pin0()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6D, 0x7F, 0xF7 }, 0xF0, 0x6E);
            // Pin 0 analog mode, value = 1023
            connection.EnqueueResponse(0x00, 0x02, 0x7F, 0x07);
            connection.EnqueueResponse(0xF7);

            PinState state = session.GetPinState(127);

            Assert.AreEqual(0, state.PinNumber);
            Assert.AreEqual(PinMode.AnalogInput, state.Mode);
            Assert.AreEqual(1023U, state.Value);
        }

        [TestMethod]
        public void GetPinState_PinNumber_Argument_Is127()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6D, 0x7F, 0xF7 }, 0xF0, 0x6E);
            // Pin 127 PWM mode, value = 256
            connection.EnqueueResponse(0x7F, 0x03, 0x00, 0x02);
            connection.EnqueueResponse(0xF7);

            PinState state = session.GetPinState(127);

            Assert.AreEqual(127, state.PinNumber);
            Assert.AreEqual(PinMode.PwmOutput, state.Mode);
            Assert.AreEqual(256U, state.Value);
        }

        [TestMethod]
        public void GetPinStateAsync()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6D, 0x05, 0xF7 }, 0xF0, 0x6E);
            // Pin 5 Digital Input mode, value = 1
            connection.EnqueueResponse(0x05, 0x00, 0x01);
            connection.EnqueueResponse(0xF7);

            PinState state = session.GetPinStateAsync(5).Result;

            Assert.AreEqual(5, state.PinNumber);
            Assert.AreEqual(PinMode.DigitalInput, state.Mode);
            Assert.AreEqual(1U, state.Value);
        }

        [TestMethod]
        public void RequestPinState()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6D, 0x01, 0xF7 }, 0xF0, 0x6E);
            // Pin 1 Digital Output mode, value = 0
            connection.EnqueueResponse(0x01, 0x01, 0x00);
            connection.EnqueueResponse(0xF7);

            session.RequestPinState(1);
            FirmataMessage message = _messagesReceived.Dequeue();

            Assert.AreEqual(MessageType.PinStateResponse, message.Type);
            PinState state = (PinState)message.Value;

            Assert.AreEqual(1, state.PinNumber);
            Assert.AreEqual(PinMode.DigitalOutput, state.Mode);
            Assert.AreEqual(0, state.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPinMode_PinNumber_Negative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF4, 0x00, (byte)PinMode.DigitalInput });
            session.SetDigitalPinMode(-1, PinMode.DigitalInput);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPinMode_PinNumber_Is128()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF4, 0x00, (byte)PinMode.DigitalInput });
            session.SetDigitalPinMode(128, PinMode.DigitalInput);
        }

        [TestMethod]
        public void SetDigitalPinMode_SetPin0ToDigitalOutput()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF4, 0x00, (byte)PinMode.DigitalOutput });
            session.SetDigitalPinMode(0, PinMode.DigitalOutput);
        }

        [TestMethod]
        public void SetDigitalPinMode_SetPin127ToAnalog()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF4, 0x7F, (byte)PinMode.AnalogInput });
            session.SetDigitalPinMode(127, PinMode.AnalogInput);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPin_PinNumber_Argument_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xE0, 0x00, 0x00 });
            session.SetDigitalPin(-1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPin_PinNumber_Argument_Is128()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6F, 0x10, 0x00, 0x00, 0xF7 });
            session.SetDigitalPin(128, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPin_Level_Argument_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xE0, 0x00, 0x00 });
            session.SetDigitalPin(0, -1);
        }

        [TestMethod]
        public void SetDigitalPin_Level_PinNumber_Argument_Is0()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xE0, 0x00, 0x00 });
            session.SetDigitalPin(0, 0);
        }

        [TestMethod]
        public void SetDigitalPin_Level_Is0_PinNumber_Argument_Is16()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x6F, 0x10, 0x00, 0x00, 0xF7);
            session.SetDigitalPin(16, 0);
        }

        [TestMethod]
        public void SetDigitalPin_Level_PinNumber_Argument_Is15()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xEF, 0x00, 0x00 });
            session.SetDigitalPin(15, 0);
        }

        [TestMethod]
        public void SetDigitalPin_Level_PinNumber_Argument_Is0_Level_Argument_Is0x3FFF()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xE0, 0x7F, 0x7F });
            session.SetDigitalPin(0, 0x3FFF);
        }

        [TestMethod]
        public void SetDigitalPin_Level_PinNumber_Argument_Is0_Level_Argument_Is0x4000()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6F, 0x00, 0x00, 0x00, 0x01, 0xF7 });
            session.SetDigitalPin(0, 0x4000);
        }

        [TestMethod]
        public void SetDigitalPin_Level_PinNumber_Argument_Is0_Level_Argument_Is0xFFFF()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x6F, 0x00, 0x7F, 0x7F, 0x03, 0xF7 });
            session.SetDigitalPin(0, 0xFFFF);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPort_PortNumber_Argument_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0x90, 0x00, 0x00 });
            session.SetDigitalPort(-1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPort_PortNumber_Argument_Is128()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0x90, 0x00, 0x00 });
            session.SetDigitalPort(-1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPort_Pins_Argument_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0x90, 0x00, 0x00 });
            session.SetDigitalPort(0, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalPort_Pins_Argument_Is256()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0x90, 0x00, 0x00 });
            session.SetDigitalPort(0, 256);
        }

        [TestMethod]
        public void SetDigitalPort_PortNumber_Is0_Pins_Is0()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0x90, 0x00, 0x00 });
            session.SetDigitalPort(0, 0);
        }

        [TestMethod]
        public void SetDigitalPort_PortNumber_Is15_Pins_Is255()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0x9F, 0x7F, 0x01 });
            session.SetDigitalPort(15, 255);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalReportMode_PortNumber_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xD0, 0x00 });
            session.SetDigitalReportMode(-1, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetDigitalReportMode_PortNumber_Is16()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xD0, 0x00 });
            session.SetDigitalReportMode(16, false);
        }

        [TestMethod]
        public void SetDigitalReportMode_PortNumber_Is1_Enable()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xD1, 0x01 });
            session.SetDigitalReportMode(1, true);
        }

        [TestMethod]
        public void SetDigitalReportMode_PortNumber_Is0_Disable()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xD0, 0x00 });
            session.SetDigitalReportMode(0, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetAnalogReportMode_Channel_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xC0, 0x00 });
            session.SetAnalogReportMode(-1, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetAnalogReportMode_Channel_Is16()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xC0, 0x00 });
            session.SetAnalogReportMode(16, false);
        }

        [TestMethod]
        public void SetAnalogReportMode_Channel_Is0_Enable()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xC0, 0x01 });
            session.SetAnalogReportMode(0, true);
        }

        [TestMethod]
        public void SetAnalogReportMode_Channel_Is15_Disable()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xCF, 0x00 });
            session.SetAnalogReportMode(15, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetSamplingInterval_Interval_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x7A, 0x00, 0x00, 0xF7 });
            session.SetSamplingInterval(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetSamplingInterval_Interval_Is0x4000()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x7A, 0x00, 0x00, 0xF7 });
            session.SetSamplingInterval(0x4000);
        }

        [TestMethod]
        public void SetSamplingInterval_Interval_Is0()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x7A, 0x00, 0x00, 0xF7 });
            session.SetSamplingInterval(0);
        }

        [TestMethod]
        public void SetSamplingInterval_Interval_Is0x3FFF()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x7A, 0x7F, 0x7F, 0xF7 });
            session.SetSamplingInterval(0x3FFF);
        }

        [TestMethod]
        public void AnalogStateReceived_MinValues()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);
            int eventHits = 0;

            session.AnalogStateReceived += (o, e) => 
            {
                Assert.AreEqual(0, e.Value.Channel);
                Assert.AreEqual(0, e.Value.Level);
                eventHits++;
            };

            session.DigitalStateReceived += (o, e) =>
            {
                Assert.Fail("Analog Message Digital processed.");
            };

            connection.EnqueueRequestAndResponse(new byte[] { 0xC0, 0x01 }, 0xE0, 0x00, 0x00);
            session.SetAnalogReportMode(0, true);
            Assert.AreEqual(1, eventHits, "AnalogStateReceived event not hit once.");
        }

        [TestMethod]
        public void AnalogStateReceived_MaxValues()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);
            int eventHits = 0;

            session.AnalogStateReceived += (o, e) =>
            {
                Assert.AreEqual(15, e.Value.Channel);
                Assert.AreEqual(0x3FFF, e.Value.Level);
                eventHits++;
            };

            session.DigitalStateReceived += (o, e) =>
            {
                Assert.Fail("Analog Message Digital processed.");
            };

            connection.EnqueueRequestAndResponse(new byte[] { 0xC0, 0x01 }, 0xEF, 0x7F, 0x7F);
            session.SetAnalogReportMode(0, true);
            Assert.AreEqual(1, eventHits, "AnalogStateReceived event not hit once.");
        }

        [TestMethod]
        public void AnalogStateReceived_Channel_LsbMsbOrder()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);
            int eventHits = 0;

            session.AnalogStateReceived += (o, e) =>
            {
                Assert.AreEqual(1, e.Value.Channel);
                Assert.AreEqual(0x1345, e.Value.Level);
                eventHits++;
            };

            session.DigitalStateReceived += (o, e) =>
            {
                Assert.Fail("Analog Message Digital processed.");
            };

            connection.EnqueueRequestAndResponse(new byte[] { 0xC0, 0x01 }, 0xE1, 0x45, 0x26);
            session.SetAnalogReportMode(0, true);
            Assert.AreEqual(1, eventHits, "DigitalStateReceived event not hit once.");
        }

        [TestMethod]
        public void DigitalStateReceived_MinValues()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);
            int eventHits = 0;

            session.AnalogStateReceived += (o, e) =>
            {
                Assert.Fail("Digital Message Analog processed.");
            };

            session.DigitalStateReceived += (o, e) =>
            {
                Assert.AreEqual(0, e.Value.Port);
                Assert.AreEqual(0, e.Value.Pins);
                eventHits++;
            };

            connection.EnqueueRequestAndResponse(new byte[] { 0xD0, 0x01 }, 0x90, 0x00, 0x00);
            session.SetDigitalReportMode(0, true);
            Assert.AreEqual(1, eventHits, "AnalogStateReceived event not hit once.");
        }

        [TestMethod]
        public void DigitalStateReceived_MaxValues()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);
            int eventHits = 0;

            session.AnalogStateReceived += (o, e) =>
            {
                Assert.Fail("Digital Message Analog processed.");
            };

            session.DigitalStateReceived += (o, e) =>
            {
                Assert.AreEqual(15, e.Value.Port);
                Assert.AreEqual(255, e.Value.Pins);
                eventHits++;
            };

            connection.EnqueueRequestAndResponse(new byte[] { 0xDF, 0x01 }, 0x9F, 0x7F, 0x01);
            session.SetDigitalReportMode(15, true);
            Assert.AreEqual(1, eventHits, "DigitalStateReceived event not hit once.");
        }

        [TestMethod]
        public void SendStringData()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(new byte[] { 0xF0, 0x71 });
            connection.EnqueueRequest("Test".To14BitIso());
            connection.EnqueueRequest(new byte[] { 0xF7 });
            session.SendStringData("Test");
        }

        [TestMethod]
        public void SendStringData_NullString()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x71, 0xF7);
            session.SendStringData(null);
        }

        [TestMethod]
        public void Receive_StringDataMessage()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);
            int eventHits = 0;

            session.MessageReceived += (o, e) =>
            {
                Assert.AreEqual(MessageType.StringData, e.Value.Type);
                Assert.AreEqual("Hello!", ((StringData)e.Value.Value).Text);
                eventHits++;
            };

            connection.EnqueueRequest(0xF0, 0x71, 0xF7);
            connection.EnqueueResponse(0xF0, 0x71, 0x48, 0x00, 0x65, 0x00, 0x6C, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x21, 0x00, 0xF7);
            session.SendStringData(null);
            Assert.AreEqual(1, eventHits, "MessageReceived event not hit once.");
        }
        
        //[TestMethod]
        //[ExpectedException(typeof(TimeoutException))]
        public void TimedoutResponse()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection, 550);

            connection.EnqueueRequest(0xF0, 0x6B, 0xF7);

            session.GetBoardCapability();
        }

        [TestMethod]
        public void MixedOrderResponses()
         {
             var connection = new MockSerialConnection();
             var session = CreateFirmataSession(connection, 3);

            // We get first ProtocolVersion response and then FirmwareResponse
            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x79, 0xF7 },
                new byte[]
                {
                    0xF9, 0x02, 0x04, 0xF0, 
                    0x79, 
                    0x01, 0x03,                    
                    0x74, 0x00, 0x65, 0x00, 0x73, 0x00, 0x74, 0x00,
                    0xF7
                });
             var f = session.GetFirmware();
             Assert.AreEqual(1, f.MajorVersion);
             Assert.AreEqual(3, f.MinorVersion);
             Assert.AreEqual("test", f.Name);
         }


        private IFirmataProtocol CreateFirmataSession(ISerialConnection connection, int timeout = -1)
        {
            var session = new ArduinoSession(connection);
            session.TimeOut = timeout;
            session.MessageReceived += (o, e) =>
                {
                    _messagesReceived.Enqueue(e.Value);
                };
            return session;
        }
    }
}
