using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Solid.Arduino.Test
{
    [TestClass]
    public class StringExtensionsTester
    {
        [TestMethod]
        public void ToBinaryCodedDecimal()
        {
            byte[] getal = "34".ToBinaryCodedDecimal();
            Assert.AreEqual(1, getal.Length);
            Assert.AreEqual(0x34, getal[0]);
        }

        [TestMethod]
        public void ToBinaryCodedDecimal_AllDigits()
        {
            byte[] getal = "0123456789".ToBinaryCodedDecimal();
            Assert.AreEqual(5, getal.Length);
            Assert.AreEqual(0x01, getal[0]);
            Assert.AreEqual(0x23, getal[1]);
            Assert.AreEqual(0x45, getal[2]);
            Assert.AreEqual(0x67, getal[3]);
            Assert.AreEqual(0x89, getal[4]);
        }

        [TestMethod]
        public void ToBinaryCodedDecimal_AllDigits_LittleEndian()
        {
            byte[] getal = "0123456789".ToBinaryCodedDecimal(true);
            Assert.AreEqual(5, getal.Length);
            Assert.AreEqual(0x98, getal[0]);
            Assert.AreEqual(0x76, getal[1]);
            Assert.AreEqual(0x54, getal[2]);
            Assert.AreEqual(0x32, getal[3]);
            Assert.AreEqual(0x10, getal[4]);
        }

        [TestMethod]
        public void ToBinaryCodedDecimal_OneDigit()
        {
            byte[] getal = "5".ToBinaryCodedDecimal();
            Assert.AreEqual(1, getal.Length);
            Assert.AreEqual(0x05, getal[0]);
        }

        [TestMethod]
        public void ToBinaryCodedDecimal_LittleEndian()
        {
            byte[] getal = "67".ToBinaryCodedDecimal(true);
            Assert.AreEqual(1, getal.Length);
            Assert.AreEqual(0x76, getal[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ToBinaryCodedDecimal_NullString()
        {
            string g = null;
            byte[] getal = g.ToBinaryCodedDecimal();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ToBinaryCodedDecimal_AlphaNumeric()
        {
            string g = "987A33";
            byte[] getal = g.ToBinaryCodedDecimal();
        }

        [TestMethod]
        public void ToBinaryCodedDecimal_EmptyString()
        {
            byte[] getal = string.Empty.ToBinaryCodedDecimal();
            Assert.AreEqual(0, getal.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void To14BitIso_NullString()
        {
            string g = null;
            byte[] getal = g.To14BitIso();
            Assert.AreEqual(0, getal.Length);
        }

        [TestMethod]
        public void To14BitIso_EmptyString()
        {
            byte[] getal = string.Empty.To14BitIso();
            Assert.AreEqual(0, getal.Length);
        }

        [TestMethod]
        public void To14BitIso_SingleCharacter()
        {
            byte[] getal = "A".To14BitIso();
            Assert.AreEqual(2, getal.Length);
            Assert.AreEqual(65, getal[0]);
            Assert.AreEqual(0, getal[1]);
        }

        [TestMethod]
        public void To14BitIso_3Characters()
        {
            byte[] getal = "ABC".To14BitIso();
            Assert.AreEqual(6, getal.Length);
            Assert.AreEqual(65, getal[0]);
            Assert.AreEqual(0, getal[1]);
            Assert.AreEqual(66, getal[2]);
            Assert.AreEqual(0, getal[3]);
            Assert.AreEqual(67, getal[4]);
            Assert.AreEqual(0, getal[5]);
        }
    }
}
