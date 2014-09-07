// -----------------------------------------------------------------------------------
// <copyright file="BinaryFixture.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// -----------------------------------------------------------------------------------

namespace NMemory.Test
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Data;

    [TestClass]
    public class BinaryFixture
    {

        [TestMethod]
        public void CastBinary()
        {
            Binary binary = new byte[] { 1, 2, 3 };

            byte[] array = (byte[])binary;

            Assert.AreEqual(array.Length, 3);
            Assert.AreEqual(array[0], 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void CastBinaryObject()
        {
            Binary binary = new byte[] { 1, 2, 3 };
            object binaryObj = binary;

            byte[] array = (byte[])binaryObj;
        }

        [TestMethod]
        public void CastBinaryObjectWorkaround()
        {
            Binary binary = new byte[] { 1, 2, 3 };
            object binaryObj = binary;

            byte[] array = (byte[])(Binary)binaryObj;

            Assert.AreEqual(array.Length, 3);
            Assert.AreEqual(array[0], 1);
        }

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

        [TestMethod]
        public void BinaryAsArgument()
        {
            Binary binary = new byte[] { };

            Method(binary);
        }

        [TestMethod]
        public void BinaryHashCode()
        {
            Binary binary = new byte[] { 1, 2, 3, 4, 5, 6, 7 };

            binary.GetHashCode();
        }

        [TestMethod]
        public void BinaryCompare1()
        {
            Binary binary1 = new byte[] { };
            Binary binary2 = new byte[] { };

            int res = binary1.CompareTo(binary2);

            Assert.AreEqual(0, res);
        }

        [TestMethod]
        public void BinaryCompare2()
        {
            Binary binary1 = new byte[] { 1 };
            Binary binary2 = new byte[] { };

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(0 < res);
        }

        [TestMethod]
        public void BinaryCompare3()
        {
            Binary binary1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 21, 22, 31 };
            Binary binary2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 21, 22, 31 };

            int res = binary1.CompareTo(binary2);

            Assert.AreEqual(0, res);
        }

        [TestMethod]
        public void BinaryCompare4()
        {
            Binary binary1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 21, 22, 31 };
            Binary binary2 = new byte[] { 1, 2, 3, 5, 5, 6, 7, 8, 11, 12, 13, 14, 21, 22, 31 };

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(res < 0);
        }

        [TestMethod]
        public void BinaryCompare5()
        {
            Binary binary1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 21, 22, 31 };
            Binary binary2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 13, 13, 14, 21, 22, 31 };

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(res < 0);
        }

        [TestMethod]
        public void BinaryCompare6()
        {
            Binary binary1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 21, 22, 31 };
            Binary binary2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 21, 23, 31 };

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(res < 0);
        }

        [TestMethod]
        public void BinaryCompare7()
        {
            Binary binary1 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 21, 22, 31 };
            Binary binary2 = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 11, 12, 13, 14, 21, 22, 32 };

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(res < 0);
        }

        [TestMethod]
        public void BinaryCompare8()
        {
            Binary binary1 = new byte[] { };
            Binary binary2 = null;

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(0 < res);
        }

        [TestMethod]
        public void BinaryCompare9()
        {
            Binary binary1 = new byte[] { };
            byte[] binary2 = null;

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(0 < res);
        }

        [TestMethod]
        public void BinaryCompare10()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 1 };

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(res == 0);
        }

        [TestMethod]
        public void BinaryCompare11()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 2 };

            int res = binary1.CompareTo(binary2);

            Assert.IsTrue(res < 0);
        }

        [TestMethod]
        public void BinaryGreaterThan1()
        {
            Binary binary1 = new byte[] { 2 };
            byte[] binary2 = new byte[] { 1 };

            bool res = binary1 > binary2;

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void BinaryGreaterThan2()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 1 };

            bool res = binary1 > binary2;

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void BinaryGreaterThan3()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 2 };

            bool res = binary1 > binary2;

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void BinaryGreaterThanOrEqual1()
        {
            Binary binary1 = new byte[] { 2 };
            byte[] binary2 = new byte[] { 1 };

            bool res = binary1 >= binary2;

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void BinaryGreaterThanOrEqual2()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 1 };

            bool res = binary1 >= binary2;

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void BinaryGreaterThanOrEqual3()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 2 };

            bool res = binary1 >= binary2;

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void BinarySmallerThan1()
        {
            Binary binary1 = new byte[] { 2 };
            byte[] binary2 = new byte[] { 1 };

            bool res = binary1 < binary2;

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void BinarySmallerThan2()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 1 };

            bool res = binary1 < binary2;

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void BinarySmallerThan3()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 2 };

            bool res = binary1 < binary2;

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void BinarySmallerThanOrEqual1()
        {
            Binary binary1 = new byte[] { 2 };
            byte[] binary2 = new byte[] { 1 };

            bool res = binary1 <= binary2;

            Assert.IsFalse(res);
        }

        [TestMethod]
        public void BinarySmallerThanOrEqual2()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 1 };

            bool res = binary1 <= binary2;

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void BinarySmallerThanOrEqual3()
        {
            Binary binary1 = new byte[] { 1 };
            byte[] binary2 = new byte[] { 2 };

            bool res = binary1 <= binary2;

            Assert.IsTrue(res);
        }

        private void Method(byte[] binary) { }
    }
}
