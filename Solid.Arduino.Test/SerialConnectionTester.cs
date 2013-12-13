using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class SerialConnectionTester
    {
        [TestMethod]
        public void SerialConnection_Constructor_WithoutParameters()
        {
            var connection = new SerialConnection();
            Assert.AreEqual(100, connection.ReadTimeout);
            Assert.AreEqual(100, connection.WriteTimeout);
        }

        [TestMethod]
        public void SerialConnection_Constructor_WithParameters()
        {
            var connection = new SerialConnection("COM1", SerialBaudRate.Bps_115200);
            Assert.AreEqual(100, connection.ReadTimeout);
            Assert.AreEqual(100, connection.WriteTimeout);
        }

        [TestMethod]
        public void SerialConnection_OpenAndClose()
        {
            var connection = new SerialConnection();
            connection.Open();
            connection.Close();
        }

        [TestMethod]
        public void SerialConnection_OpenAndDoubleClose()
        {
            var connection = new SerialConnection();
            connection.Open();
            connection.Close();
            connection.Close();
        }
    }
}
