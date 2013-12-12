using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solid.Arduino;

namespace Solid.Arduino.Test
{
    /// <summary>
    /// Summary description for IStringProtocolTester
    /// </summary>
    [TestClass]
    public class IStringProtocolTester
    {
        public IStringProtocolTester()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void Write()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            connection.EnqueueStringRequest("Test!");
            session.Write("Test!");
        }

        [TestMethod]
        public void Write_NoArgument()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            session.Write();
        }

        [TestMethod]
        public void Write_NullString()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            session.Write(null);
        }

        [TestMethod]
        public void Write_EmptyString()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            session.Write(string.Empty);
        }

        [TestMethod]
        public void WriteLine()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            connection.EnqueueStringRequest("Test!\n");
            session.WriteLine("Test!");
        }

        [TestMethod]
        public void WriteLine_NoArgument()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            connection.EnqueueStringRequest(session.NewLine);
            session.WriteLine();
        }

        [TestMethod]
        public void WriteLine_NullString()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            connection.EnqueueStringRequest(session.NewLine);
            session.WriteLine(null);
        }

        [TestMethod]
        public void WriteLine_EmptyString()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            connection.EnqueueStringRequest(session.NewLine);
            session.WriteLine(string.Empty);
        }

        [TestMethod]
        public void WriteLine_CarriageReturnAsNewLine()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            connection.EnqueueStringRequest("\r");
            session.NewLine = "\r";
            session.WriteLine();
        }

        [TestMethod]
        public void Read_OneCharacter()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            connection.EnqueueStringResponse("Z");
            connection.Receive();
            //string data = session.Read();
            //Assert.Equals("Z", data);
        }

        private IStringProtocol CreateSerialSession(ISerialConnection connection)
        {
            var session = new ArduinoSession(connection);
            session.MessageReceived += (o, e) =>
                {
                    Assert.Fail("MessageReceived event triggered");
                };
            session.AnalogStateReceived += (o, e) =>
                {
                    Assert.Fail("AnalogStateReceived event triggered");
                };
            session.DigitalStateReceived += (o, e) =>
                {
                    Assert.Fail("DigitalStateReceived event triggered");
                };
            session.I2cReplyReceived += (o, e) =>
                {
                    Assert.Fail("I2cReplyReceived event triggered");
                };
            return session;
        }
    }
}
