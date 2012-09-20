using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Data;
using NMemory.Test.Environment.Data;
using NMemory.Tables;

namespace NMemory.Test
{
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
    }
}
