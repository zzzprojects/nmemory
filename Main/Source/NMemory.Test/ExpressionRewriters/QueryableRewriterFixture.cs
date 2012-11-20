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
    using NMemory.Linq;
    using NMemory.Test.Environment.Data;
    using NMemory.Utilities;

    [TestClass]
    public class QueryableRewriterFixture
    {
        [TestMethod]
        public void QueryableRewriter_RemoveAsQueryable()
        {
            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpression(() => db.Groups.SelectAll().AsQueryable().Where(t => t.Id > 2).Select(t => t.Name));
            Expression expectedExpression = CreateExpression(() => db.Groups.SelectAll().Where(t => t.Id > 2).Select(t => t.Name));

            IExpressionRewriter rewriter = new QueryableRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void QueryableRewriter_DoNotRemoveAsQueryable()
        {
            // TODO: Implement
        }

        [TestMethod]
        public void QueryableRewriter_ContainsNonGenericMethod()
        {
            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpression(
                () => db.Groups.Select(g => string.Concat("Name: ", g.Name)));

            Expression expectedExpression = expression;

            IExpressionRewriter rewriter = new QueryableRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void QueryableRewriter_MoveAsQueryable()
        {
            // TODO: Implement
        }

        private static Expression CreateExpression<TResult>(Expression<Func<TResult>> expression)
        {
            return expression.Body;
        }
    }
}
