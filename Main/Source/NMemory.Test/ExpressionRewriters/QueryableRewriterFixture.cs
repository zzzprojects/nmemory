using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Execution.Optimization;
using NMemory.Execution.Optimization.Rewriters;
using NMemory.Linq;
using NMemory.Test.Environment.Data;
using NMemory.Utilities;

namespace NMemory.Test.ExpressionRewriters
{
    [TestClass]
    public class QueryableRewriterFixture
    {
        [TestMethod]
        public void QueryableRewriter_RemoveAsQueryable()
        {
            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpresion(() => db.Groups.SelectAll().AsQueryable().Where(t => t.Id > 2).Select(t => t.Name));
            Expression expectedExpression = CreateExpresion(() => db.Groups.SelectAll().Where(t => t.Id > 2).Select(t => t.Name));

            IExpressionRewriter rewriter = new QueryableRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void QueryableRewriter_DoNotRemoveAsQueryable()
        {
            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpresion(
                () => db.Groups.SelectAll().AsQueryable().LeftOuterJoin(db.Members,
                    g => g.Id,
                    m => m.GroupId,
                    (g, m) => new { member = m.Name, group = g.Name }).Select(x => x.member));
            Expression expectedExpression = expression;

            IExpressionRewriter rewriter = new QueryableRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        [TestMethod]
        public void QueryableRewriter_MoveAsQueryable()
        {
            TestDatabase db = new TestDatabase();
            Expression expression = CreateExpresion(
                () => db.Groups.SelectAll().AsQueryable().Where(t => t.Id > 2).LeftOuterJoin(db.Members,
                    g => g.Id,
                    m => m.GroupId,
                    (g, m) => new { member = m.Name, group = g.Name }).Select(x => x.member));
            Expression expectedExpression = CreateExpresion(
                () => db.Groups.SelectAll().Where(t => t.Id > 2).AsQueryable().LeftOuterJoin(db.Members,
                    g => g.Id,
                    m => m.GroupId,
                    (g, m) => new { member = m.Name, group = g.Name }).Select(x => x.member));

            IExpressionRewriter rewriter = new QueryableRewriter();
            expression = rewriter.Rewrite(expression);

            Assert.AreEqual(expectedExpression.ToString(), expression.ToString());
        }

        private static Expression CreateExpresion<TResult>(Expression<Func<TResult>> expression)
        {
            return expression.Body;
        }
    }
}
