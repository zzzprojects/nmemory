// -----------------------------------------------------------------------------------
// <copyright file="JoinFixture.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// -----------------------------------------------------------------------------------

namespace NMemory.Test
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Test.Environment.Data;

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
