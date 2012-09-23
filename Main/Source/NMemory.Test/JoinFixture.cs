using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Test.Environment.Data;

namespace NMemory.Test
{
    [TestClass]
    public class JoinFixture
    {
        [TestMethod]
        public void OuterJoinNulledFields()
        {
            TestDatabase db = new TestDatabase();
            db.AddMemberGroupRelation();

            Group group = new Group { Name = "Group" };
            db.Groups.Insert(group);

            db.Members.Insert(new Member { Id = "A", Name = "John", GroupId = null });
            db.Members.Insert(new Member { Id = "B", Name = "Kay", GroupId = group.Id });

            var q =
                from m in db.Members
                join g_ in db.Groups on m.GroupId equals g_.Id into groups_
                from g in groups_.DefaultIfEmpty()
                select new { Name = m.Name, GroupName = g.Name };

            var result = q.ToList();

            Assert.IsTrue(result.Any(x => x.GroupName == null));
        }
    }
}
