using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.StoredProcedures;
using NMemory.Test.Environment.Data;

namespace NMemory.Test
{
    [TestClass]
    public class StoredProcedureFixture
    {

        [TestMethod]
        public void StoredProcedureParameterDescription()
        {
            TestDatabase database = new TestDatabase();

            IQueryable<Group> query = database.Groups.Where(g => g.Id > new Parameter<int>("param1") + (int)new Parameter<long>("param2"));

            var procedure = database.StoredProcedures.Create(query);

            Assert.AreEqual(procedure.Parameters.Count, 2);
            Assert.IsTrue(procedure.Parameters.Any(p => p.Name == "param1" && p.Type == typeof(int)));
            Assert.IsTrue(procedure.Parameters.Any(p => p.Name == "param2" && p.Type == typeof(long)));
        }

        [TestMethod]
        public void StoredProcedureInternalParameter()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            IQueryable<Group> query = database.Groups.Where(g => g.Id > new Parameter<int>("param1"));

            var procedure = database.StoredProcedures.Create(query);

            var result = procedure.Execute(new Dictionary<string, object> { { "param1", 1 } }).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(g => g.Id > 1));
        }

        [TestMethod]
        public void SharedStoredProcedure()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            var procedure = new SharedStoredProcedure<TestDatabase, Group>(
                d => d.Groups.Where(g => g.Id > 1));

            var result = procedure.Execute(database, null).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(g => g.Id > 1));
        }
    }
}
