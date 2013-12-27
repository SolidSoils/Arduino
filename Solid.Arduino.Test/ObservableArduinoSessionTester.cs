using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class ObservableArduinoSessionTester
    {
        [TestMethod]
        public void TestMethod1()
        {
            var x = new ArduinoSession(new MockSerialConnection());

            var tracker = x.CreateAnalogStateMonitor();
            
        }
    }
}
