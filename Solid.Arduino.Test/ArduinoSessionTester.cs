using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino.Firmata;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class ArduinoSessionTester
    {

        private readonly Queue<FirmataMessage> _messagesReceived = new Queue<FirmataMessage>();

        [TestMethod]
        public void CreateSessionWithClosedConnection()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            Assert.AreEqual(true, connection.IsOpen);
        }

        //[TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArduinoSession_Constructor_NullArgument()
        {
            var session = new ArduinoSession(null);
        }

        [TestMethod]
        public void CreateSessionWithOpenConnection()
        {
            var connection = new MockSerialConnection();
            connection.Open();
            var session = new ArduinoSession(connection);
            Assert.AreEqual(true, connection.IsOpen);
        }

        [TestMethod]
        public void ClearSession()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            session.Clear();
            session.Dispose();
        }

        [TestMethod]
        public void TimeOutReached()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            session.TimeOut = 1;
            // TODO: verder uitwerken.
        }

        [TestMethod]
        public void TimeOut_GetAndSet()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            Assert.AreEqual(-1, session.TimeOut);
            session.TimeOut = 7;
            Assert.AreEqual(7, session.TimeOut);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TimeOut_SetNegative()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            session.TimeOut = -2;
        }

        [TestMethod]
        public void Dispose()
        {
            var connection = new MockSerialConnection();
            var session = new ArduinoSession(connection);
            session.Dispose();
        }

        [TestMethod]
        public void Dispose_OnOpenConnection()
        {
            var connection = new MockSerialConnection();
            connection.Open();
            var session = new ArduinoSession(connection);
            session.Dispose();
        }

        private void session_OnMessageReceived(object par_Sender, FirmataMessageEventArgs par_EventArgs)
        {
            _messagesReceived.Enqueue(par_EventArgs.Value);
        }
    }
}
