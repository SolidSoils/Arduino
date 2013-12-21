using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino.Firmata;
using Solid.Arduino.I2c;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class I2cProtocolTester
    {
        private readonly Queue<FirmataMessage> _messagesReceived = new Queue<FirmataMessage>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetI2cReadInterval_Interval_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x78, 0x00, 0x00, 0xF7 });
            session.SetI2cReadInterval(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetI2cReadInterval_Interval_Is0x4000()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x78, 0x00, 0x00, 0xF7 });
            session.SetI2cReadInterval(0x4000);
        }

        [TestMethod]
        public void SetI2cReadInterval_Interval_Is0()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x78, 0x00, 0x00, 0xF7 });
            session.SetI2cReadInterval(0);
        }

        [TestMethod]
        public void SetI2cReadInterval_Interval_Is0x3FFF()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF0, 0x78, 0x7F, 0x7F, 0xF7 });
            session.SetI2cReadInterval(0x3FFF);
        }

        [TestMethod]
        public void WriteI2c_SlaveAddress_Parameter_Is0()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x00, 0x7F, 0x01, 0xF7);
            session.WriteI2c(0, 255);
        }

        [TestMethod]
        public void WriteI2c_SlaveAddress_Parameter_Is0x3FF()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x7F, 0x27, 0x7F, 0x01, 0xF7);
            session.WriteI2c(0x3FF, 255);
        }

        [TestMethod]
        public void WriteI2c_SlaveAddress_Parameter_Is127()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x7F, 0x00, 0x11, 0x00, 0x22, 0x00, 0x33, 0x00, 0x44, 0x00, 0x7F, 0x01, 0xF7);
            session.WriteI2c(127, 0x11, 0x22, 0x33, 0x44, 0xFF);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WriteI2c_SlaveAddress_Parameter_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x02, 0x21, 0xF7);
            session.WriteI2c(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void WriteI2c_SlaveAddress_Parameter_Is0x400()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x02, 0x21, 0xF7);
            session.WriteI2c(0x400);
        }

        [TestMethod]
        public void WriteI2c_Data_Parameter_IsNull()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x02, 0x21, 0xF7);
            session.WriteI2c(130);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadI2cOnce_SlaveAddress_Parameter_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x08, 0x00, 0x00, 0xF7);
            session.ReadI2cOnce(-1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadI2cOnce_SlaveAddress_Parameter_Is0x400()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x08, 0x00, 0x00, 0xF7);
            session.ReadI2cOnce(0x400, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadI2cOnce_BytesToRead_Parameter_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x08, 0x00, 0x00, 0xF7);
            session.ReadI2cOnce(0, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadI2cOnce_BytesToRead_Parameter_Is0x4000()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x08, 0x00, 0x00, 0xF7);
            session.ReadI2cOnce(0, 0x4000);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadI2cOnce_SlaveRegister_Parameter_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x08, 0x00, 0x00, 0xF7);
            session.ReadI2cOnce(0, -1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadI2cOnce_SlaveRegister_Parameter_Is0x4000()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x08, 0x00, 0x00, 0xF7);
            session.ReadI2cOnce(0, 0x4000, 1);
        }

        [TestMethod]
        public void ReadI2cOnce_MinimumParameters()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x08, 0x00, 0x00, 0xF7);
            session.ReadI2cOnce(0, 0);
        }

        [TestMethod]
        public void ReadI2cOnce_MaximumParameters()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x7F, 0x2F, 0x7F, 0x7F, 0xF7);
            session.ReadI2cOnce(0x3FF, 0x3FFF);
        }

        [TestMethod]
        public void ReadI2cOnce_MinimumParameters_WithSlaveRegister()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0xF7);
            session.ReadI2cOnce(0, 0, 0);
        }

        [TestMethod]
        public void ReadI2cOnce_MaximumParameters_WithSlaveRegister()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x7F, 0x2F, 0x7F, 0x7F, 0x7F, 0x7F, 0xF7);
            session.ReadI2cOnce(0x3FF, 0x3FFF, 0x3FFF);
        }

        [TestMethod]
        public void ReadI2cContinous()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x45, 0x10, 0x7F, 0x01, 0xF7);
            session.ReadI2cContinuous(0x45, 255);
        }

        [TestMethod]
        public void ReadI2cContinous_WithSlaveRegister()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x45, 0x10, 0x07, 0x0C, 0x7F, 0x01, 0xF7);
            session.ReadI2cContinuous(0x45, 0x0607, 255);
        }

        [TestMethod]
        public void GetI2cReply()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x01, 0x08, 0x03, 0x00, 0xF7);
            connection.EnqueueResponse(0xF0, 0x77, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x06, 0x00, 0x07, 0x00, 0xF7);
            I2cReply reply = session.GetI2cReply(1, 3);
            Assert.AreEqual(1, reply.Address);
            Assert.AreEqual(0, reply.Register);
            Assert.AreEqual(3, reply.Data.Length);
            Assert.AreEqual(5, reply.Data[0]);
            Assert.AreEqual(6, reply.Data[1]);
            Assert.AreEqual(7, reply.Data[2]);
        }

        [TestMethod]
        public void GetI2cReply_BytesToRead_Parameter_Is0()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x01, 0x08, 0x00, 0x00, 0xF7);
            connection.EnqueueResponse(0xF0, 0x77, 0x01, 0x00, 0x00, 0x00, 0xF7);
            I2cReply reply = session.GetI2cReply(1, 0);
            Assert.AreEqual(1, reply.Address);
            Assert.AreEqual(0, reply.Register);
            Assert.AreEqual(0, reply.Data.Length);
        }

        [TestMethod]
        public void GetI2cReply_WithSlaveRegister()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x01, 0x08, 0x02, 0x00, 0x01, 0x00, 0xF7);
            connection.EnqueueResponse(0xF0, 0x77, 0x01, 0x00, 0x02, 0x00, 0x04, 0x00, 0xF7);
            I2cReply reply = session.GetI2cReply(1, 2, 1);
            Assert.AreEqual(1, reply.Address);
            Assert.AreEqual(2, reply.Register);
            Assert.AreEqual(1, reply.Data.Length);
            Assert.AreEqual(4, reply.Data[0]);
        }

        [TestMethod]
        public void GetI2cReplyAsync()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x7F, 0x08, 0x02, 0x00, 0xF7);
            connection.EnqueueResponse(0xF0, 0x77, 0x7F, 0x00, 0x00, 0x00, 0x7F, 0x01, 0x7E, 0x01, 0xF7);
            I2cReply reply = session.GetI2cReplyAsync(127, 2).Result;
            Assert.AreEqual(127, reply.Address);
            Assert.AreEqual(0, reply.Register);
            Assert.AreEqual(2, reply.Data.Length);
            Assert.AreEqual(255, reply.Data[0]);
            Assert.AreEqual(254, reply.Data[1]);
        }

        [TestMethod]
        public void GetI2cReplyAsync_WithSlaveRegister()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x7F, 0x08, 0x04, 0x02, 0x02, 0x00, 0xF7);
            connection.EnqueueResponse(0xF0, 0x77, 0x7F, 0x00, 0x04, 0x02, 0x7F, 0x01, 0x7E, 0x01, 0xF7);
            I2cReply reply = session.GetI2cReplyAsync(127, 260, 2).Result;
            Assert.AreEqual(127, reply.Address);
            Assert.AreEqual(260, reply.Register);
            Assert.AreEqual(2, reply.Data.Length);
            Assert.AreEqual(255, reply.Data[0]);
            Assert.AreEqual(254, reply.Data[1]);
        }

        [TestMethod]
        public void I2cReplyReceived()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);
            int eventHits = 0;

            session.I2cReplyReceived += (o, e) =>
            {
                Assert.AreEqual(1, e.Value.Address);
                Assert.AreEqual(0, e.Value.Register);
                Assert.AreEqual(3, e.Value.Data.Length);
                Assert.AreEqual(5, e.Value.Data[0]);
                Assert.AreEqual(6, e.Value.Data[1]);
                Assert.AreEqual(7, e.Value.Data[2]);
                eventHits++;
            };

            connection.EnqueueRequest(0xF0, 0x76, 0x01, 0x08, 0x03, 0x00, 0xF7);
            connection.EnqueueResponse(0xF0, 0x77, 0x01, 0x00, 0x00, 0x00, 0x05, 0x00, 0x06, 0x00, 0x07, 0x00, 0xF7);
            I2cReply reply = session.GetI2cReply(1, 3);
            Assert.AreEqual(1, eventHits);
        }

        [TestMethod]
        public void StopI2cReading()
        {
            var connection = new MockSerialConnection();
            var session = CreateFirmataSession(connection);

            connection.EnqueueRequest(0xF0, 0x76, 0x00, 0x18, 0xF7);
            session.StopI2cReading();
        }

        private II2cProtocol CreateFirmataSession(ISerialConnection connection, int timeout = -1)
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
