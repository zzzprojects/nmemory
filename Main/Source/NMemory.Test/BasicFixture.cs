using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Tables;
using NMemory.Test.Environment.Data;
using NMemory.Linq;

namespace NMemory.Test
{
    [TestClass]
    public class BasicFixture
    {
        [TestMethod]
        public void CreateDatabase()
        {
            Database database = new Database();
        }

        [TestMethod]
        public void CreateTable()
        {
            Database database = new Database();

            database.Tables.Create<Group, int>(g => g.Id);
        }

        [TestMethod]
        public void CreateTestDatabase()
        {
            TestDatabase database = new TestDatabase();
        }

        [TestMethod]
        public void InsertEntity()
        {
            TestDatabase database = new TestDatabase();

            database.Members.Insert(new Member { Id = "JD1967", Name = "John Doe" });

            var result = database.Members.ToList();
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void UpdateEntity()
        {
            TestDatabase database = new TestDatabase();
            Member member = new Member { Id = "JD1967", Name = "Joh Doe" };
            database.Members.Insert(member);

            member.Name = "John Doe";

            database.Members.Update(member);

            Member updated = database.Members.FirstOrDefault();
            Assert.AreEqual(updated.Name, "John Doe");
        }

        [TestMethod]
        public void UpdateEntities()
        {
            TestDatabase database = new TestDatabase();
            
            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            database.Groups.Update(x => new Group { Name = x.Name + " (deleted)" });

            var newEntities = database.Groups.ToList();
            Assert.IsTrue(newEntities.All(e => e.Name.EndsWith("(deleted)")));
        }


        [TestMethod]
        public void DeleteEntity()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });

            database.Groups.Delete(new Group() { Id = 1 });

            Group group = database.Groups.FirstOrDefault();

            Assert.AreEqual(database.Groups.Count, 1);
            Assert.IsNotNull(group);
            Assert.AreEqual(group.Name, "Group 2");
        }

        [TestMethod]
        public void DeleteEntities()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            database.Groups.Delete();

            Group group = database.Groups.FirstOrDefault();

            Assert.AreEqual(database.Groups.Count, 0);
        }


        [TestMethod]
        public void EnumerateTable()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.ToList();
        }
        

        [TestMethod]
        public void QueryTable()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Where(x => true).ToList();
        }
    }
}
