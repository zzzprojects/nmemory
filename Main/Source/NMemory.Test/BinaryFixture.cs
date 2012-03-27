using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Data;

namespace NMemory.Test
{
    [TestClass]
    public class BinaryFixture
    {
        [TestMethod]
        public void SetBinaryValue()
        {
            Binary binary = new byte[] { 1, 2, 3 };

            Assert.AreEqual(binary.Length, 3);
            Assert.AreEqual(binary[0], 1);
        }

        [TestMethod]
        public void SetNullBinaryValue()
        {
            byte[] array = null;
            Binary binary = array;

            Assert.IsNull(binary);
        }

        [TestMethod]
        public void GetBinaryValue()
        {
            Binary binary = new byte[] { 1, 2, 3 };

            byte[] array = binary;

            Assert.AreEqual(array.Length, 3);
            Assert.AreEqual(array[0], 1);
        }

        [TestMethod]
        public void GetNullBinaryValue()
        {
            Binary binary = null;
            byte[] array = binary;

            Assert.IsNull(array);
        }

        [TestMethod]
        public void CompareBinary1()
        {
            Binary binary1 = new byte[] { 1, 2, 3 };
            Binary binary2 = new byte[] { 1, 2, 3 };

            Assert.AreEqual(binary1, binary2);
        }

        [TestMethod]
        public void CompareBinary2()
        {
            Binary binary1 = new byte[] { 1, 2, 3 };
            byte[] binary2 = new byte[] { 1, 2, 3 };

            Assert.AreEqual(binary1, binary2);
        }

        [TestMethod]
        public void CompareBinary3()
        {
            Binary binary1 = new byte[] { 1, 2, 3 };
            Binary binary2 = new byte[] { 1, 2, 4 };

            Assert.AreNotEqual(binary1, binary2);
        }

        [TestMethod]
        public void CompareBinary4()
        {
            Binary binary1 = new byte[] { 1, 2, 3 };
            byte[] binary2 = new byte[] { 1, 2, 4 };

            Assert.AreNotEqual(binary1, binary2);
        }
    }
}
