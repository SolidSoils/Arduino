using System;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino;

namespace Solid.Arduino.IntegrationTest
{
    /// <summary>
    /// Performs tests with a connected Arduino device.
    /// </summary>
    [TestClass]
    public class SerialConnectionTester
    {
        /// <summary>
        /// Finds a live serial connection by issuing a Firmata SysEx Firmware query (0xF0 0x79 0xF7).
        /// </summary>
        /// <remarks>
        /// Requires sketch StandardFirmata.ino to run on the connected device.
        /// </remarks>
        [TestMethod]
        public void FindSerialConnection_FirmataEnabled()
        {
            using (var arduinoConnection = SerialConnection.FindSerialConnection())
            {
                Assert.IsNotNull(arduinoConnection);
            }
        }

        /// <summary>
        /// Finds a live serial connection by issuing a query string.
        /// </summary>
        /// <remarks>
        /// Requires sketch SerialReply.ino to run on the connected device.
        /// (This sketch can be found in this project.)
        /// </remarks>
        [TestMethod]
        public void FindSerialConnection_Serial()
        {
            using (var arduinoConnection = SerialConnection.FindSerialConnection("Hello?", "Arduino!"))
            {
                Assert.IsNotNull(arduinoConnection);
            }
        }
    }
}
