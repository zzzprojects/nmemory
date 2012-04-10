using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Data;
using NMemory.Test.Data;
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
            Database database = new Database();
            ITable<TimestampEntity> table = database.Tables.Create<TimestampEntity, int>(e => e.Id);

            TimestampEntity entity = new TimestampEntity() { Id = 1 };

            table.Insert(entity);
        }
    }
}
