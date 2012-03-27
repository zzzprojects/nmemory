using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Test.Data;

namespace NMemory.Test
{
    [TestClass]
    public class RelationFixture
    {
        [TestMethod]
        public void CreateRelation()
        {
            TestDatabase database = new TestDatabase();

            database.AddMemberGroupRelation();
        }

        [TestMethod]
        public void InsertRelationItem()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });
        }

        [TestMethod]
        public void UpdateRelationItem()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });

            database.Members.Update(new Member { Id = "JD" }, new Member { Id = "JD", Name = "John Doe", GroupId = 2 });
        }


    }
}
