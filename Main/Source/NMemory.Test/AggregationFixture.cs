using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Test.Environment.Data;

namespace NMemory.Test
{
    [TestClass]
    public class AggregationFixture
    {
        [TestMethod]
        public void Count()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            int count = database.Groups.Where(g => g.Id > 1).Count();

            Assert.AreEqual(count, 2);
        }
    }
}
