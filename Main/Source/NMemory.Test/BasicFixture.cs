// -----------------------------------------------------------------------------------
// <copyright file="BasicFixture.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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
    using NMemory.Linq;
    using NMemory.Test.Environment.Data;

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

            database.Tables.Create<Group, int>(g => g.Id, null);
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
