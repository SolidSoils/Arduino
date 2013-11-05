using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino.Firmata;

namespace Solid.Arduino.Firmata.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var connection = new SerialConnection();
            var session = new FirmataSession(connection);

            session.OnMessageReceived += session_OnMessageReceived;

            session.GetFirmware();

            Console.ReadLine();
        }

        void session_OnMessageReceived(object par_Sender, FirmataMessageEventArgs par_EventArgs)
        {
            if (par_EventArgs.Value.Type == MessageType.FirmwareResponse)
            {
                var firmware = (Firmware)par_EventArgs.Value.Value;
                Console.WriteLine("Version: {0}.{1}", firmware.MajorVersion, firmware.MinorVersion);
                Console.WriteLine("Name: {0}", firmware.Name);
            }
        }
    }
}
