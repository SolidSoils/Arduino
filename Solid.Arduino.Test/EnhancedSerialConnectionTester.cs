using System;
using System.IO.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class EnhancedSerialConnectionTester
    {
        [TestMethod]
        public void EnhancedSerialConnection_Constructor_WithoutParameters()
        {
            if (!AreSerialPortsAvailable())
            {
                Assert.ThrowsException<ArgumentNullException>(() => new SerialConnection());
                return;
            }

            var connection = new EnhancedSerialConnection();
            Assert.AreEqual(100, connection.ReadTimeout);
            Assert.AreEqual(100, connection.WriteTimeout);
            Assert.AreEqual(115200, connection.BaudRate);
        }

        [TestMethod]
        public void EnhancedSerialConnection_Constructor_WithParameters()
        {
            if (!AreSerialPortsAvailable())
                return;

            var connection = new EnhancedSerialConnection("COM1", SerialBaudRate.Bps_115200);
            Assert.AreEqual(100, connection.ReadTimeout);
            Assert.AreEqual(100, connection.WriteTimeout);
            Assert.AreEqual(115200, connection.BaudRate);
        }

        [TestMethod]
        public void EnhancedSerialConnection_OpenAndClose()
        {
            if (!AreSerialPortsAvailable())
                return;

            var connection = new EnhancedSerialConnection();
            connection.Open();
            connection.Close();
        }

        [TestMethod]
        public void EnhancedSerialConnection_OpenAndDoubleClose()
        {
            if (!AreSerialPortsAvailable())
                return;

            var connection = new EnhancedSerialConnection();
            connection.Open();
            connection.Close();
            connection.Close();
        }

        private static bool AreSerialPortsAvailable()
        {
            return SerialPort.GetPortNames().Length > 0;
        }
    }
}
