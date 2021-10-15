using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Solid.Arduino.IntegrationTest
{
    /// <summary>
    /// Summary description for SocketConnectionTester
    /// </summary>
    [TestClass]
    public class SocketConnectionTester
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
        public void TestMethod1()
        {
            var permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts);

            IPHostEntry ipHost = Dns.GetHostEntry("");
            var endpoint = new IPEndPoint(ipHost.AddressList[0], 4510);
            var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(endpoint);
            

        }


    }
}
