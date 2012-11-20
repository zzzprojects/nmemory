// -----------------------------------------------------------------------------------
// <copyright file="QueryableRewriterFixture.cs" company="NMemory Team">
//     Copyright (C) 2012 NMemory Team
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
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class OuterJoinLogicalRewriterFixture
    {
        [TestMethod]
        public void OuterJoinLogicalRewriter_SingleFieldKey()
        {
            // NOTE: some parameter names are hard coded into the optimizer 

            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpression(() =>
                db.Members.SelectMany(m =>
                    db.Groups.Where(g => g.Id == m.GroupId).DefaultIfEmpty(),
                    (m, g) => new { Member = m, Group = g }));

            Expression expectedExpression = CreateExpression(() =>
                db.Members.GroupJoin(
                    db.Groups,
                    outer => outer.GroupId,
                    inner => inner.Id,
                    (outer, inner) => JoinGroup.Create(outer, inner))
                .SelectMany(
                    group => group.Inner.DefaultIfEmpty(),
                    (group, inner) => new { Member = group.Outer, Group = inner }));

            IExpressionRewriter rewriter = new OuterJoinLogicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.Type, expression.Type);
            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void OuterJoinLogicalRewriter_DoubleFieldKey()
        {
            // NOTE: some parameter names are hard coded into the optimizer 

            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpression(() =>
                db.Members.SelectMany(m =>
                    db.Groups
                        .Where(g => g.Id == m.GroupId && m.GroupId2 == g.Id2)
                        .DefaultIfEmpty(),
                    (m, g) => new { Member = m, Group = g }));

            Expression expectedExpression = CreateExpression(() =>
                db.Members.GroupJoin(
                    db.Groups,
                    outer => new Tuple<int?, int>(outer.GroupId, outer.GroupId2),
                    inner => new Tuple<int?, int>(inner.Id, inner.Id2),
                    (outer, inner) => JoinGroup.Create(outer, inner))
                .SelectMany(
                    group => group.Inner.DefaultIfEmpty(),
                    (group, inner) => new { Member = group.Outer, Group = inner }));

            IExpressionRewriter rewriter = new OuterJoinLogicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.Type, expression.Type);
            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void OuterJoinLogicalRewriter_DoubleJoin()
        {
            // NOTE: some parameter names are hard coded into the optimizer 

            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpression(() =>
                db.Members.SelectMany(outer =>
                    db.Groups
                        .Where(g => g.Id == outer.GroupId)
                        .DefaultIfEmpty(),
                    (outer, inner) => 
                        new 
                        { 
                            Member = outer, 
                            Group = inner 
                        })
               .SelectMany(outer =>
                    db.Groups
                        .Where(g => g.Id == outer.Member.GroupId)
                        .DefaultIfEmpty(),
                    (outer, inner) => 
                        new 
                        {
                            Member = outer.Member,
                            Group1 = outer.Group,
                            Group2 = inner
                        }));

            Expression expectedExpression = CreateExpression(() =>
                db.Members.GroupJoin(
                    db.Groups,
                    outer => outer.GroupId,
                    inner => inner.Id,
                    (outer, inner) => JoinGroup.Create(outer, inner))
                .SelectMany(
                    group => group.Inner.DefaultIfEmpty(),
                    (group, inner) =>
                        new
                        {
                            Member = group.Outer,
                            Group = inner
                        })
                .GroupJoin(
                    db.Groups,
                    outer => outer.Member.GroupId,
                    inner => inner.Id,
                    (outer, inner) => JoinGroup.Create(outer, inner))
                .SelectMany(
                    group => group.Inner.DefaultIfEmpty(),
                    (group, inner) =>
                        new
                        {
                            Member = group.Outer.Member,
                            Group1 = group.Outer.Group,
                            Group2 = inner
                        }));

            IExpressionRewriter rewriter = new OuterJoinLogicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.Type, expression.Type);
            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void OuterJoinLogicalRewriter_TripleJoin()
        {
            // NOTE: some parameter names are hard coded into the optimizer 

            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpression(() =>
                db.Members.SelectMany(outer =>
                    db.Groups
                        .Where(g => g.Id == outer.GroupId)
                        .DefaultIfEmpty(),
                    (outer, inner) =>
                        new
                        {
                            Member = outer,
                            Group = inner
                        })
               .SelectMany(outer =>
                    db.Groups
                        .Where(g => g.Id == outer.Member.GroupId)
                        .DefaultIfEmpty(),
                    (outer, inner) =>
                        new
                        {
                            Member = outer.Member,
                            Group1 = outer.Group,
                            Group2 = inner
                        })
               .SelectMany(outer =>
                    db.Groups
                        .Where(g => g.Id == outer.Member.GroupId)
                        .DefaultIfEmpty(),
                    (outer, inner) =>
                        new
                        {
                            Member = outer.Member,
                            Group1 = outer.Group1,
                            Group2 = outer.Group2,
                            Group3 = inner
                        }));

            Expression expectedExpression = CreateExpression(() =>
                db.Members.GroupJoin(
                    db.Groups,
                    outer => outer.GroupId,
                    inner => inner.Id,
                    (outer, inner) => JoinGroup.Create(outer, inner))
                .SelectMany(
                    group => group.Inner.DefaultIfEmpty(),
                    (group, inner) =>
                        new
                        {
                            Member = group.Outer,
                            Group = inner
                        })
                .GroupJoin(
                    db.Groups,
                    outer => outer.Member.GroupId,
                    inner => inner.Id,
                    (outer, inner) => JoinGroup.Create(outer, inner))
                .SelectMany(
                    group => group.Inner.DefaultIfEmpty(),
                    (group, inner) =>
                        new
                        {
                            Member = group.Outer.Member,
                            Group1 = group.Outer.Group,
                            Group2 = inner
                        })
                .GroupJoin(
                    db.Groups,
                    outer => outer.Member.GroupId,
                    inner => inner.Id,
                    (outer, inner) => JoinGroup.Create(outer, inner))
                .SelectMany(
                    group => group.Inner.DefaultIfEmpty(),
                    (group, inner) =>
                        new
                        {
                            Member = group.Outer.Member,
                            Group1 = group.Outer.Group1,
                            Group2 = group.Outer.Group2,
                            Group3 = inner
                        }));

            IExpressionRewriter rewriter = new OuterJoinLogicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.Type, expression.Type);
            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        private static Expression CreateExpression<TResult>(Expression<Func<TResult>> expression)
        {
            return expression.Body;
        }
    }
}
