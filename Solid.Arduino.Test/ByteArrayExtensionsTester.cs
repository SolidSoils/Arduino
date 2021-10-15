using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Solid.Arduino.Firmata;

namespace Solid.Arduino.Test
{
    /// <summary>
    /// Summary description for ByteArrayExtensionsTester
    /// </summary>
    [TestClass]
    public class ByteArrayExtensionsTester
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConvertBinaryCodedDecimalToString_NullData()
        {
            byte[] data = null;
            data.ConvertBinaryCodedDecimalToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertBinaryCodedDecimalToString_NonDigitBytes()
        {
            byte[] data = new byte[] { 0x99, 0xA0, 0x0A };
            data.ConvertBinaryCodedDecimalToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertBinaryCodedDecimalToString_LeastSignificantNotDigit()
        {
            byte[] data = new byte[] { 0x34, 0x5A, 0x98 };
            data.ConvertBinaryCodedDecimalToString();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertBinaryCodedDecimalToString_MostSignificantNotDigit()
        {
            byte[] data = new byte[] { 0x34, 0x56, 0xE8 };
            data.ConvertBinaryCodedDecimalToString();
        }

        [TestMethod]
        public void ConvertBinaryCodedDecimalToString_NoData()
        {
            byte[] data = new byte[0];
            Assert.AreEqual(0, data.ConvertBinaryCodedDecimalToString().Length);
        }

        [TestMethod]
        public void ConvertBinaryCodedDecimalToString_1Byte()
        {
            byte[] data = new byte[]
            {
                0x12
            };

            Assert.AreEqual("12", data.ConvertBinaryCodedDecimalToString());
        }

        [TestMethod]
        public void ConvertBinaryCodedDecimalToString_AllDigits()
        {
            byte[] data = new byte[]
            {
                0x12, 0x34, 0x56, 0x78, 0x90
            };

            Assert.AreEqual("1234567890", data.ConvertBinaryCodedDecimalToString());
        }

        [TestMethod]
        public void ConvertBinaryCodedDecimalToString_AllDigits_LittleEndian()
        {
            byte[] data = new byte[]
            {
                0x12, 0x34, 0x56, 0x78, 0x90
            };

            Assert.AreEqual("0987654321", data.ConvertBinaryCodedDecimalToString(true));
        }
    }
}
