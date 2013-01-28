// -----------------------------------------------------------------------------------
// <copyright file="JoinPhysicalRewriterFixture.cs" company="NMemory Team">
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

namespace NMemory.Test.ExpressionRewriters
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Execution.Optimization;
    using NMemory.Execution.Optimization.Rewriters;
    using NMemory.Indexes;
    using NMemory.Linq;
    using NMemory.Tables;
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class JoinPhysicalRewriterFixture
    {
        [TestMethod]
        public void JoinPhysicalRewriterFixture_SingleFieldKey()
        {
            // NOTE: some parameter names are hard coded into the optimizer 

            TestDatabase db = new TestDatabase();
            ITable<Group> groups = db.Groups;
            IIndex<Group, int> index = db.Groups.PrimaryKeyIndex as IIndex<Group, int>;

            Expression expression = CreateExpression(() =>
                db.Members.Join(
                    db.Groups,
                    outer => outer.GroupId,
                    inner => inner.Id,
                    (m, g) => new { Member = m, Group = g }));

            Expression expectedExpression = CreateExpression(() =>
                db.Members.JoinIndexed(
                    index,
                    outer => outer.GroupId,
                    x => x == null,
                    x => (int)x,
                    (m, g) => new { Member = m, Group = g })
                .AsQueryable());

            IExpressionRewriter rewriter = new JoinPhysicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.Type, expression.Type);
            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void JoinPhysicalRewriterFixture_DoubleJoin()
        {
            // NOTE: some parameter names are hard coded into the optimizer 

            TestDatabase db = new TestDatabase();
            ITable<Group> groups = db.Groups;
            IIndex<Group, int> index = db.Groups.PrimaryKeyIndex as IIndex<Group, int>;

            Expression expression = CreateExpression(() =>
                db.Members.Join(
                    db.Groups,
                    outer => outer.GroupId,
                    inner => inner.Id,
                    (m, g) => new { Member = m, Group1 = g })
               .Join(
                    db.Groups,
                    outer => outer.Member.GroupId,
                    inner => inner.Id,
                    (m, g) => new { Member = m.Member, Group1 = m.Group1, Group2 = g }));

            Expression expectedExpression = CreateExpression(() =>
                db.Members.JoinIndexed(
                    index,
                    outer => outer.GroupId,
                    x => x == null,
                    x => (int)x,
                    (m, g) => new { Member = m, Group1 = g })
                .AsQueryable()
                .JoinIndexed(
                    index,
                    outer => outer.Member.GroupId,
                    x => x == null,
                    x => (int)x,
                    (m, g) => new { Member = m.Member, Group1 = m.Group1, Group2 = g })
                .AsQueryable());

            IExpressionRewriter rewriter = new JoinPhysicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.Type, expression.Type);
            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void JoinPhysicalRewriterFixture_DoubleFieldKey()
        {
            // NOTE: some parameter names are hard coded into the optimizer 

            TestDatabase db = new TestDatabase();
            db.AddMemberGroupRelation(createMultiField: true, useTuple:true);

            ITable<Group> groups = db.Groups;
            IIndex<Group, Tuple<int, int>> index = db.Groups.Indexes.Last()
                as IIndex<Group, Tuple<int, int>>;

            Expression expression = CreateExpression(() =>
                db.Members.Join(
                    db.Groups,
                    outer => new Tuple<int?, int>(outer.GroupId, outer.GroupId2),
                    inner => new Tuple<int?, int>((int?)inner.Id, inner.Id2),
                    (m, g) => new { Member = m, Group = g }));

            Expression expectedExpression = CreateExpression(() =>
                db.Members.JoinIndexed(
                    index,
                    outer => new Tuple<int?, int>(outer.GroupId, outer.GroupId2),
                    x => x.Item1 == null,
                    x => new Tuple<int, int>((int)x.Item1, x.Item2),
                    (m, g) => new { Member = m, Group = g })
                .AsQueryable());

            IExpressionRewriter rewriter = new JoinPhysicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.Type, expression.Type);
            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        private static Expression CreateExpression<TResult>(Expression<Func<TResult>> expression)
        {
            Expression expr = expression.Body;

            expr = new TableAccessRewriter().Rewrite(expr);
            expr = new IndexAccessRewriter().Rewrite(expr);

            return expr;
        }
    }
}
