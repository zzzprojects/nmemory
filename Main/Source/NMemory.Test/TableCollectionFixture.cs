using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Indexes;
using NMemory.Tables;
using NMemory.Test.Environment.Data;

namespace NMemory.Test
{
    [TestClass]
    public class TableCollectionFixture
    {
        [TestMethod]
        public void GetAllRelations()
        {
            var db = new Database();

            var members = db.Tables.Create((Member x) => x.Id, null);
            var groups = db.Tables.Create((Group x) => x.Id, null);

            var rel = db.Tables.CreateRelation(
                groups.PrimaryKeyIndex,
                members.CreateIndex(new RedBlackTreeIndexFactory(), x => x.GroupId),
                x => x.Value, x => x, new RelationOptions());

            var rel1 = db.Tables.GetAllRelations().FirstOrDefault();

            Assert.IsTrue(object.ReferenceEquals(rel1, rel));
        }

    }
}
