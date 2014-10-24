// -----------------------------------------------------------------------------------
// <copyright file="TimestampFixture.cs" company="NMemory Team">
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
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Data;
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class TimestampFixture
    {
        [TestMethod]
        public void TimestampCreation()
        {
            Timestamp timestamp1 = Timestamp.CreateNew();
            Timestamp timestamp2 = Timestamp.CreateNew();

            Assert.AreNotEqual(timestamp1, timestamp2);
        }

        [TestMethod]
        public void TimestampCopy()
        {
            Timestamp timestamp1 = new Timestamp();
            Timestamp timestamp2 = timestamp1;

            Assert.AreEqual(timestamp1, timestamp2);
        }

        [TestMethod]
        public void TimestampInsert()
        {
            TestDatabase db = new TestDatabase();

            TimestampEntity entity = new TimestampEntity() { Id = 1 };
            db.TimestampEntities.Insert(entity);
        }

        [TestMethod]
        public void TimestampChangedOnInsert()
        {
            TestDatabase db = new TestDatabase();

            TimestampEntity entity = new TimestampEntity() { Id = 1 };
            Timestamp timestamp = entity.Timestamp;

            db.TimestampEntities.Insert(entity);

            Assert.AreNotEqual(timestamp, db.TimestampEntities.Single(t => t.Id == 1).Timestamp);
        }

        [TestMethod]
        public void TimestampChangedOnUpdate()
        {
            TestDatabase db = new TestDatabase();

            TimestampEntity entity = new TimestampEntity() { Id = 1 };
            
            db.TimestampEntities.Insert(entity);
            Timestamp timestamp = entity.Timestamp;

            entity.Data = "altered";
            db.TimestampEntities.Update(entity);

            Assert.AreNotEqual(timestamp, db.TimestampEntities.Single(t => t.Id == 1).Timestamp);
        }

        [TestMethod]
        public void BinaryCompare1()
        {
            Timestamp timestamp1 = Timestamp.CreateNew();
            Timestamp timestamp2 = timestamp1;

            int res = timestamp1.CompareTo(timestamp2);

            Assert.AreEqual(0, res);
        }

        [TestMethod]
        public void BinaryCompare2()
        {
            Timestamp timestamp1 = Timestamp.CreateNew();
            Timestamp timestamp2 = Timestamp.CreateNew();

            int res = timestamp1.CompareTo(timestamp2);
            
            Assert.IsTrue(res < 0);
        }

        [TestMethod]
        public void BinaryCompare3()
        {
            Timestamp timestamp1 = Timestamp.CreateNew();
            Timestamp timestamp2 = Timestamp.CreateNew();

            int res = timestamp2.CompareTo(timestamp1);

            Assert.IsTrue(res > 0);
        }
    }
}
