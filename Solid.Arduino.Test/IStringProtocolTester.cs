using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Solid.Arduino.Test
{
    /// <summary>
    /// Summary description for IStringProtocolTester
    /// </summary>
    [TestClass]
    public class IStringProtocolTester
    {
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
        public async Task Read_OneCharacter()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = Task.Run(() =>
                {
                    string data = session.Read();
                    Assert.AreEqual("Z", data);
                }
            );

            connection.MockReceiveDelayed("Zxxxx");

            await t;
        }

        //[TestMethod]
        public void StringReceived()
        {
            string s = null;

            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);
            session.NewLine = "\n";
            session.StringReceived += (o, e) =>
                {
                    s = e.Text;
                };

            connection.MockReceiveDelayed("ACME\n");
            Assert.AreEqual("ACME", s);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public async Task ReceivedStringBufferOverflow()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = Task.Run(() =>
                {
                    string data = session.Read(2048);
                }
            );

            connection.MockReceiveDelayed(new string('*', 40480));

            await t;
        }

        //[TestMethod] // Verdacht
        public async Task RequestBufferOverflow()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var tasks = new List<Task<string>>(100);

            for (int x = 0; x < 100; x++)
                tasks.Add(session.ReadAsync());

            Thread.Sleep(250);

            connection.MockReceiveDelayed(new string('*', 100));

            await Task.WhenAll(tasks);

            foreach (Task<string> t in tasks)
            {
                Assert.AreEqual("*", t.Result);
            }
        }

        [TestMethod]
        public async Task Read_StringBlock()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = Task.Run(() =>
                {
                    string data = session.Read(6);
                    Assert.AreEqual("Hello!", data);
                }
            );

            connection.MockReceiveDelayed("Hello!!!");

            await t;
        }

        //[TestMethod]
        //[ExpectedException(typeof(TimeoutException))]
        public async Task Read_WithTimeout()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection, 550);
            
            var t = Task.Run(() =>
                {
                    string data = session.Read(6);
                    Assert.AreEqual("Hello!", data);
                }
            );

            await t;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task Read_NegativeLength()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = Task.Run(() =>
                {
                    string data = session.Read(-1);
                }
            );

            connection.MockReceiveDelayed("Hello!!!");

            await t;
        }

        [TestMethod]
        public async Task ReadLine()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = Task.Run(() =>
                {
                    string data = session.ReadLine();
                    Assert.AreEqual("A line.", data);
                }
            );

            connection.MockReceiveDelayed("A line." + session.NewLine + " Trailing text.");

            await t;
        }

        [TestMethod]
        public async Task ReadLine_EmptyString()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = Task.Run(() =>
                {
                    string data = session.ReadLine();
                    Assert.AreEqual(string.Empty, data);
                }
            );

            connection.MockReceiveDelayed(session.NewLine);

            await t;
        }

        [TestMethod]
        public async Task ReadLine_CarriageReturn()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            session.NewLine = "\r";

            var t = Task.Run(() =>
                {
                    string data = session.ReadLine();
                    Assert.AreEqual("A line.", data);
                }
            );

            connection.MockReceiveDelayed("A line." + session.NewLine);

            await t;
        }

        [TestMethod]
        public async Task ReadLine_CarriageReturnPlusLinefeed()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            session.NewLine = "\r\n";

            var t = Task.Run(() =>
                {
                    string data = session.ReadLine();
                    Assert.AreEqual("A line.", data);
                    data = session.ReadLine();
                    Assert.AreEqual("Second line.", data);
                }
            );

            connection.MockReceiveDelayed("A line." + session.NewLine + "Second line." + session.NewLine);

            await t;
        }

        [TestMethod]
        public async Task ReadTo()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = Task.Run(() =>
                {
                    string data = session.ReadTo('.');
                    Assert.AreEqual("A line", data);
                }
            );

            connection.MockReceiveDelayed("A line.");

            await t;
        }

        [TestMethod]
        public async Task ReadTo_EmptyString()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = Task.Run(() =>
                {
                    string data = session.ReadTo(';');
                    Assert.AreEqual(string.Empty, data);
                }
            );

            connection.MockReceiveDelayed(";");

            await t;
        }

        [TestMethod]
        public async Task ReadAsync_NoParameters()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = session.ReadAsync();
            connection.MockReceiveDelayed("Test");

            string data = await t;
            Assert.AreEqual("T", data);
        }

        [TestMethod]
        public async Task ReadAsync_4Characters()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = session.ReadAsync(4);
            connection.MockReceiveDelayed("Test");

            string data = await t;
            Assert.AreEqual("Test", data);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task ReadAsync_Length_IsNegative()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = session.ReadAsync(-1);
            connection.MockReceiveDelayed("Test");

            string data = await t;
        }

        [TestMethod]
        public async Task ReadLineAsync()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = session.ReadLineAsync();
            connection.MockReceiveDelayed("Test" + session.NewLine);
            
            string data = await t;
            Assert.AreEqual("Test", data);
        }

        [TestMethod]
        public async Task ReadToAsync()
        {
            var connection = new MockSerialConnection();
            var session = CreateSerialSession(connection);

            var t = session.ReadToAsync(',');
            connection.MockReceiveDelayed("alpha,beta,gamma");

            string data = await t;
            Assert.AreEqual("alpha", data);
        }

        private IStringProtocol CreateSerialSession(ISerialConnection connection, int timeout = -1)
        {
            var session = new ArduinoSession(connection);
            session.TimeOut = timeout;
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
            session.I2CReplyReceived += (o, e) =>
                {
                    Assert.Fail("I2CReplyReceived event triggered");
                };
            session.StringReceived += (o, e) =>
                {
                    Console.WriteLine("Received: '{0}'", e.Text);
                };
            return session;
        }

        private async Task<string> Read()
        {
            return await Task.Run<string>(() => "Test");
        }
    }
}
