// ----------------------------------------------------------------------------------
// <copyright file="EntityTypeSearchVisitorFixture.cs" company="NMemory Team">
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
// ----------------------------------------------------------------------------------

namespace NMemory.Test.Visitors
{
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Common;
    using NMemory.Common.Visitors;
    using NMemory.Indexes;
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class EntityTypeSearchVisitorFixture
    {
        [TestMethod]
        public void SearchConstantTable()
        {
            EntityTypeSearchVisitor search = new EntityTypeSearchVisitor();

            var db = new Database();
            var table = db.Tables.Create<Member, string>(x => x.Id, null);

            Expression expr = Expression.Constant(table);

            search.Visit(expr);

            Assert.AreEqual(1, search.FoundEntityTypes.Length);
            Assert.AreEqual(typeof(Member), search.FoundEntityTypes[0]);
        }

        [TestMethod]
        public void SearchIndex()
        {
            EntityTypeSearchVisitor search = new EntityTypeSearchVisitor();

            var db = new Database();
            var table = db.Tables.Create<Member, string>(x => x.Id, null);
            var index = table.CreateIndex(new RedBlackTreeIndexFactory(), x => x.GroupId);

            Expression expr = Expression.Constant(index);

            search.Visit(expr);

            Assert.AreEqual(1, search.FoundEntityTypes.Length);
            Assert.AreEqual(typeof(Member), search.FoundEntityTypes[0]);
        }

        [TestMethod]
        public void SearchTableCollectionFindTable()
        {
            EntityTypeSearchVisitor search = new EntityTypeSearchVisitor();

            var db = new Database();

            Expression expr = Expression.Call(
                Expression.Property(
                    Expression.Constant(db),
                    DatabaseMembers.Database_Tables),
                DatabaseMembers.TableCollection_FindTable,
                Expression.Constant(typeof(Member)));

            search.Visit(expr);

            Assert.AreEqual(1, search.FoundEntityTypes.Length);
            Assert.AreEqual(typeof(Member), search.FoundEntityTypes[0]);
        }

        [TestMethod]
        public void SearchTableCollectionFindTableExtension()
        {
            EntityTypeSearchVisitor search = new EntityTypeSearchVisitor();

            var db = new Database();

            Expression expr = Expression.Call(
                null,
                DatabaseMembers
                    .TableCollectionExtensions_FindTable
                    .MakeGenericMethod(typeof(Member)),
                Expression.Property(
                    Expression.Constant(db),
                    DatabaseMembers.Database_Tables));

            search.Visit(expr);

            Assert.AreEqual(1, search.FoundEntityTypes.Length);
            Assert.AreEqual(typeof(Member), search.FoundEntityTypes[0]);
        }     
    }
}
