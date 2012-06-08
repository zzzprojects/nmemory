using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Test.Data;
using NMemory.Tables;

namespace NMemory.Test
{
    [TestClass]
    public class IdentityFixture
    {
        [TestMethod]
        public void CreateTableWithIdentity()
        {
            Database database = new Database();

            database.Tables.Create<Group, int>(g => g.Id, new IdentitySpecification<Group>(g => g.Id));
        }

        [TestMethod]
        public void InsertEntityWithIdentity()
        {
            Database database = new Database();

            Table<Group, int> table = database.Tables.Create<Group, int>(g => g.Id, new IdentitySpecification<Group>(g => g.Id));

            Group group1 = new Group { Name = "Group 1" };
            Group group2 = new Group { Name = "Group 2" };

            table.Insert(group1);
            table.Insert(group2);

            Assert.AreEqual(group1.Id + 1, group2.Id);
        }

        [TestMethod]
        public void InsertEntityWithIdentityAndInitialData()
        {
            Database database = new Database();

            Group[] initialData = {
                new Group { Id = 1, Name = "Group 1" }, 
                new Group { Id = 2, Name = "Group 2" } };

            Table<Group, int> table = database.Tables.Create<Group, int>(g => g.Id, new IdentitySpecification<Group>(g => g.Id), initialData);

            Group group = new Group { Name = "Group 3" };

            table.Insert(group);

            Assert.AreEqual(3, group.Id);
        }

        [TestMethod]
        public void ReflectionInsertEntityWithIdentityAndInitialData()
        {
            Database database = new Database();

            Group[] initialData = {
                new Group { Id = 1, Name = "Group 1" }, 
                new Group { Id = 2, Name = "Group 2" } };

            Table<Group, int> table = database.Tables.Create<Group, int>(g => g.Id, new IdentitySpecification<Group>(g => g.Id), initialData);

            Group group = new Group { Name = "Group 3" };

            ((IReflectionTable)table).Insert(group);

            Assert.AreEqual(3, group.Id);
        }
    }
}
