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
    public class InnerJoinLogicalRewriterFixture
    {
        [TestMethod]
        public void InnerJoinLogicalRewriter_SingleFieldKey()
        {
            // NOTE: inner and outer names are hard coded, so it is required to use them as
            // prediction

            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpresion(() =>
                from outer in db.Members
                from inner in db.Groups.Where(g => g.Id == outer.GroupId)
                select new { outer, inner });

            Expression expectedExpression = CreateExpresion(() => 
                from outer in db.Members
                join inner in db.Groups on outer.GroupId equals inner.Id
                select new { outer, inner });

            IExpressionRewriter rewriter = new InnerJoinLogicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void InnerJoinLogicalRewriter_MultiFieldKey()
        {
            // NOTE: inner and outer names are hard coded, so it is required to use them as
            // prediction

            TestDatabase db = new TestDatabase();

            Expression expression = CreateExpresion(() =>
                from outer in db.Members
                from inner in db.Groups.Where(g => 
                    outer.GroupId == g.Id &&
                    g.Id2 == outer.GroupId2)
                select new { outer, inner });

            Expression expectedExpression = CreateExpresion(() =>
                from outer in db.Members
                join inner in db.Groups on 
                    new Tuple<int?, int>(outer.GroupId, outer.GroupId2) equals
                    new Tuple<int?, int>(inner.Id, inner.Id2)
                select new { outer, inner });

            IExpressionRewriter rewriter = new InnerJoinLogicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void InnerJoinLogicalRewriter_MultiJoin()
        {
            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpresion(() =>
                from outer in db.Members
                from inner1 in db.Groups.Where(g => g.Id == outer.GroupId)
                from inner2 in db.Groups.Where(g => g.Id == outer.GroupId)
                select new { outer, inner1, inner2 });

            Expression expectedExpression = CreateExpresion(() =>
                from outer in db.Members
                join inner1 in db.Groups on outer.GroupId equals inner1.Id
                join inner2 in db.Groups on outer.GroupId equals inner2.Id
                select new { outer, inner1, inner2 });

            IExpressionRewriter rewriter = new InnerJoinLogicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(
                FormatExpressionString(expectedExpression.ToString()),
                FormatExpressionString(expression.ToString()));
        }

        [TestMethod]
        public void InnerJoinLogicalRewriter_OmitRewrite()
        {
            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpresion(() =>
                db.Members.SelectMany(
                    m => db.Groups
                        .Where(g => g.Name.Length > m.GroupId)
                        .Where(g => g.Id == m.GroupId),
                    (m, g) => new { m, g }));

            string expected = expression.ToString();

            IExpressionRewriter rewriter = new InnerJoinLogicalRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(
                expected,
                expression.ToString());
        }

        private static string FormatExpressionString(string value)
        {
            // Fix parameter names to ensure the expression strings equality
            return value
                .Replace("inner1", "inner")
                .Replace("inner2", "inner")
                .Replace("<>h__TransparentIdentifiera", "outer")
                .Replace("<>h__TransparentIdentifier8", "outer");
        }

        private static Expression CreateExpresion<TResult>(Expression<Func<TResult>> expression)
        {
            return expression.Body;
        }
    }
}
