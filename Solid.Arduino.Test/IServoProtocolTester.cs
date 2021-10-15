using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino.Firmata;
using Solid.Arduino.Firmata.Servo;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class IServoProtocolTester
    {
        private readonly Queue<FirmataMessage> _messagesReceived = new Queue<FirmataMessage>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConfigureServo_PinNumber_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00 });
            session.ConfigureServo(-1, 0, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConfigureServo_PinNumber_Is128()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00 });
            session.ConfigureServo(128, 0, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConfigureServo_MinPulse_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00 });
            session.ConfigureServo(0, -1, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConfigureServo_MinPulse_Is0x4000()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00 });
            session.ConfigureServo(0, 0x4000, 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConfigureServo_MaxPulse_Is0x4000()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF7 });
            session.ConfigureServo(15, 0, 0x4000);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConfigureServo_MaxPulse_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF7 });
            session.ConfigureServo(15, 0, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConfigureServo_MinPulse_GT_MaxPulse()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF7 });
            session.ConfigureServo(1, 0x3FFF, 0x3FFE);
        }

        [TestMethod]
        public void ConfigureServo_MinPulse_EQ_MaxPulse()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x01, 0x01, 0x02, 0x01, 0x02, 0xF7 });
            session.ConfigureServo(1, 257, 257);
        }

        [TestMethod]
        public void ConfigureServo()
        {
            var connection = new MockSerialConnection();
            var session = CreateServoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x70, 0x05, 0x00, 0x02, 0x01, 0x02, 0xF7 });
            session.ConfigureServo(5, 256, 257);
        }

        private IServoProtocol CreateServoSession(ISerialConnection connection, int timeout = -1)
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
