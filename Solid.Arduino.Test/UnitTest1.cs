using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino.Firmata;
using System.Threading.Tasks;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class UnitTest1
    {

        private readonly Queue<FirmataMessage> _messagesReceived = new Queue<FirmataMessage>();

        [TestMethod]
        public void ClearSession()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            session.Clear();
            session.Dispose();
        }

        [TestMethod]
        public void ResetBoard()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            session.OnMessageReceived += session_OnMessageReceived;
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
            var session = new ArduinoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF9 }, new byte[] { 0xF9, 2, 3 });

            ProtocolVersion version = session.GetProtocolVersion();
            Assert.AreEqual(2, version.Major);
            Assert.AreEqual(3, version.Minor);
        }

        [TestMethod]
        public void GetProtocolVersionAsync()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);

            connection.EnqueueRequestAndResponse(new byte[] { 0xF9 }, new byte[] { 0xF9, 2, 3 });
            ProtocolVersion version = session.GetProtocolVersionAsync().Result;
            Assert.AreEqual(2, version.Major);
            Assert.AreEqual(3, version.Minor);
        }

        [TestMethod]
        public void RequestProtocolVersion()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            session.OnMessageReceived += session_OnMessageReceived;

            connection.EnqueueRequestAndResponse(new byte[] { 0xF9 }, new byte[] { 0xF9, 2, 3 });

            session.RequestProtocolVersion();

            Assert.AreEqual(1, _messagesReceived.Count);
            FirmataMessage message = _messagesReceived.Dequeue();
            Assert.AreEqual(MessageType.ProtocolVersion, message.Type);
            var version = (ProtocolVersion)message.Value;
            Assert.AreEqual(2, version.Major);
            Assert.AreEqual(3, version.Minor);
        }

        private void session_OnMessageReceived(object par_Sender, FirmataMessageEventArgs par_EventArgs)
        {
            _messagesReceived.Enqueue(par_EventArgs.Value);
        }
    }
}
