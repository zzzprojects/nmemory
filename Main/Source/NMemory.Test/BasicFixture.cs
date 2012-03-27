using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Tables;
using NMemory.Test.Data;

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

            database.CreateTable<Group, int>(g => g.Id);
        }

        [TestMethod]
        public void CreateTableWithIdentity()
        {
            Database database = new Database();

            database.CreateTable<Group, int>(g => g.Id, new IdentitySpecification<Group>(g => g.Id));
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

            database.Members.Update(member, member);

            Member updated = database.Members.FirstOrDefault();
            Assert.AreEqual(updated.Name, "John Doe");
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
