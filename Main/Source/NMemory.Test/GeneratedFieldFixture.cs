using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Test.Data;
using NMemory.Data;

namespace NMemory.Test
{
    [TestClass]
    public class GeneratedFieldFixture
    {
        [TestMethod]
        public void IdentityReturned()
        {
            TestDatabase db = new TestDatabase(createIdentityForGroup:true);
            Group group = new Group { Name = "Group" };

            db.Groups.Insert(group);

            Assert.AreEqual(group.Id, 1);
        }

        [TestMethod]
        public void TimestampReturnedOnInsert()
        {
            TestDatabase db = new TestDatabase();
            TimestampEntity entity = new TimestampEntity() { Id = 1 };
            Timestamp timestamp = entity.Timestamp;

            db.TimestampEntities.Insert(entity);

            Assert.AreNotEqual(timestamp, entity.Timestamp);
        }

        [TestMethod]
        public void TimestampReturnedOnUpdate()
        {
            TestDatabase db = new TestDatabase();
            TimestampEntity entity = new TimestampEntity() { Id = 1 };

            db.TimestampEntities.Insert(entity);
            Timestamp timestamp = entity.Timestamp;

            entity.Data = "altered";
            db.TimestampEntities.Update(entity);


            Assert.AreNotEqual(timestamp, entity.Timestamp);
        }


    }
}
