using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Test.Data;
using NMemory.Exceptions;
using NMemory.Linq;

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
        public void InsertRelatedEntity()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });
        }

        [TestMethod]
        [ExpectedException(typeof(ForeignKeyViolationException))]
        public void InsertRelatedEntityViolation()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 3 });
        }

        [TestMethod]
        public void UpdateRelatedEntity()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });

            database.Members.Update(new Member { Id = "JD", Name = "John Doe", GroupId = 2 });
        }

        [TestMethod]
        [ExpectedException(typeof(ForeignKeyViolationException))]
        public void UpdateRelatedEntityViolation()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });

            database.Members.Update(new Member { Id = "JD", Name = "John Doe", GroupId = 3 });
        }

        [TestMethod]
        public void UpdateRelatedEntities()
        {
            TestDatabase database = new TestDatabase(false); // No identity
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Id = 1, Name = "Group 1" });
            database.Groups.Insert(new Group { Id = 2, Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });
            database.Members.Insert(new Member { Id = "MS", Name = "Michael Smith", GroupId = 2 });

            // Flip the IDs
            database.Groups.Update(g => new Group { Id = g.Id % 2 + 1 });
        }

        [TestMethod]
        [ExpectedException(typeof(ForeignKeyViolationException))]
        public void UpdateRelatedEntitiesViolation()
        {
            TestDatabase database = new TestDatabase(false); // No identity
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Id = 1, Name = "Group 1" });
            database.Groups.Insert(new Group { Id = 2, Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });
            database.Members.Insert(new Member { Id = "MS", Name = "Michael Smith", GroupId = 2 });

            // Increment the Ids
            database.Groups.Update(g => new Group { Id = g.Id + 1 });
        }


        [TestMethod]
        public void DeleteRelatedEntity()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });

            database.Groups.Delete(new Group { Id = 2 });
        }

        [TestMethod]
        [ExpectedException(typeof(ForeignKeyViolationException))]
        public void DeleteRelatedEntityViolation()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });

            database.Groups.Delete(new Group { Id = 1 });
        }

        [TestMethod]
        public void DeleteRelatedEntities()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });

            database.Groups.Where(g => g.Id > 1).Delete();
        }

        [TestMethod]
        [ExpectedException(typeof(ForeignKeyViolationException))]
        public void DeleteRelatedEntitiesViolation()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = 1 });

            database.Groups.Delete();
        }


    }
}
