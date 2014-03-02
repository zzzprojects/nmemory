// -----------------------------------------------------------------------------------
// <copyright file="PrimitiveKeyInfoServiceFixture.cs" company="NMemory Team">
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

namespace NMemory.Test.Services
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Data;
    using NMemory.Indexes;
    using NMemory.Services;
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class PrimitiveKeyInfoServiceFixture
    {
        [TestMethod]
        public void PrimitiveKeyInfoService_CreateKey()
        {
            PrimitiveKeyInfoService service = new PrimitiveKeyInfoService();

            IKeyInfo<BinaryEntity, Binary> key = null;

            bool res = service.TryCreateKeyInfo<BinaryEntity, Binary>(x => x.Binary, out key);

            Assert.IsTrue(res);
            Assert.IsNotNull(key);
            Assert.AreEqual(1, key.EntityKeyMembers.Length);
            Assert.AreEqual(typeof(Binary), key.KeyType);
        }
    }
}
