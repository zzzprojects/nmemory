// -----------------------------------------------------------------------------------
// <copyright file="RelationFixture.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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
    using NMemory.Exceptions;
    using NMemory.Linq;
    using NMemory.Test.Environment.Data;

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
        public void CreateRelationWithData()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Name = "Group 1" });

            database.Members.Insert(new Member { Id = "A", GroupId = 1 });
            database.Members.Insert(new Member { Id = "B", GroupId = null });

            database.AddMemberGroupRelation();
        }

        [TestMethod]
        [ExpectedException(typeof(ForeignKeyViolationException))]
        public void CreateRelationWithDataViolation()
        {
            TestDatabase database = new TestDatabase();

            database.Members.Insert(new Member { Id = "A", GroupId = 1 });
            database.Members.Insert(new Member { Id = "B", GroupId = null });

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
        public void InsertRelatedEntityWithEmptyForeignKey()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation();

            database.Members.Insert(new Member { Id = "JD", Name = "John Doe", GroupId = null });
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

        [TestMethod]
        public void CreateMultiFieldRelation()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation(createMultiField: true);
        }

        [TestMethod]
        [ExpectedException(typeof(ForeignKeyViolationException))]
        public void CreateMultiFieldRelationWithInconsistentData()
        {
            TestDatabase database = new TestDatabase();

            database.Members.Insert(new Member { Id = "A", GroupId = 1, GroupId2 = 2 });

            database.AddMemberGroupRelation(createMultiField: true);
        }

        [TestMethod]
        public void CreateMultiFieldRelationWithEmptyData()
        {
            TestDatabase database = new TestDatabase();

            database.Members.Insert(new Member { Id = "A", GroupId = null, GroupId2 = 2 });

            database.AddMemberGroupRelation(createMultiField: true);
        }

        [TestMethod]
        public void CreateMultiFieldRelationWithConsistentData()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Id = 1, Id2 = 2, Name = "A" });
            database.Members.Insert(new Member { Id = "A", GroupId = 1, GroupId2 = 2 });

            database.AddMemberGroupRelation(createMultiField: true);
        }


        [TestMethod]
        public void CreateSingleFieldRelationWithExpressionFactory()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation(createMultiField: false, useExpressionFactory: true);
        }

        [TestMethod]
        public void CreateMultiFieldRelationWithExpressionFactory()
        {
            TestDatabase database = new TestDatabase();
            database.AddMemberGroupRelation(createMultiField: true, useExpressionFactory: true);
        }

        [TestMethod]
        [ExpectedException(typeof(ForeignKeyViolationException))]
        public void CreateMultiFieldRelationWithInconsistentDataAndExpressionFactory()
        {
            TestDatabase database = new TestDatabase();

            database.Members.Insert(new Member { Id = "A", GroupId = 1, GroupId2 = 2 });

            database.AddMemberGroupRelation(createMultiField: true, useExpressionFactory: true);
        }
    }
}
