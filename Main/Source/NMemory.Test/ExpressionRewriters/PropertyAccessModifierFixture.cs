// -----------------------------------------------------------------------------------
// <copyright file="PropertyAccessModifierFixture.cs" company="NMemory Team">
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

namespace NMemory.Test.ExpressionRewriters
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Execution.Optimization;
    using NMemory.Execution.Optimization.Rewriters;
    using NMemory.Test.Environment.Data;
    using NMemory.Test.Environment.Utils;

    [TestClass]
    public class PropertyAccessModifierFixture
    {
        public static readonly string Static = "Static";

        [TestMethod]
        public void PropertyAccessModiferOnNullableMember()
        {
            IExpressionRewriter modifier = new PropertyAccessRewriter();
            Expression<Func<Member, int?>> expression = m => m.GroupId;

            Expression newBody = modifier.Rewrite(expression.Body);
            expression = ExpressionUtils.ChangeBody(expression, newBody);

            int? result = expression.Compile().Invoke(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void PropertyAccessModiferOnMappedNullableMember()
        {
            IExpressionRewriter modifier = new PropertyAccessRewriter();
            Expression<Func<Member, string>> expression = m => m.GroupId == null ? "null" : "not null";

            Expression newBody = modifier.Rewrite(expression.Body);
            expression = ExpressionUtils.ChangeBody(expression, newBody);

            string result = expression.Compile().Invoke(null);

            Assert.AreEqual("null", result);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void PropertyAccessModiferOnNotNullableMember()
        {
            IExpressionRewriter modifier = new PropertyAccessRewriter();
            Expression<Func<Member, int>> expression = m => m.GroupId2;

            Expression newBody = modifier.Rewrite(expression.Body);
            expression = ExpressionUtils.ChangeBody(expression, newBody);

            int? result = expression.Compile().Invoke(null);
        }

        [TestMethod]
        public void PropertyAccessModiferOnCastedMember()
        {
            IExpressionRewriter modifier = new PropertyAccessRewriter();
            Expression<Func<Member, int?>> expression = m => (int?)m.GroupId2;

            Expression newBody = modifier.Rewrite(expression.Body);
            expression = ExpressionUtils.ChangeBody(expression, newBody);

            int? result = expression.Compile().Invoke(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void PropertyAccessModiferOnCastedNullableMember()
        {
            IExpressionRewriter modifier = new PropertyAccessRewriter();
            Expression<Func<Member, int?>> expression = m => (int?)m.GroupId;

            Expression newBody = modifier.Rewrite(expression.Body);
            expression = ExpressionUtils.ChangeBody(expression, newBody);

            int? result = expression.Compile().Invoke(null);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void PropertyAccessModiferOnStaticMember()
        {
            IExpressionRewriter modifier = new PropertyAccessRewriter();
            Expression<Func<string>> expression = () => PropertyAccessModifierFixture.Static;

            Expression newBody = modifier.Rewrite(expression.Body);
            expression = ExpressionUtils.ChangeBody(expression, newBody);

            string result = expression.Compile().Invoke();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PropertyAccessModiferOnConvertedStaticMember()
        {
            IExpressionRewriter modifier = new PropertyAccessRewriter();
            Expression<Func<DateTime?>> expression = () => (DateTime?)DateTime.Now;

            Expression newBody = modifier.Rewrite(expression.Body);
            expression = ExpressionUtils.ChangeBody(expression, newBody);

            DateTime? result = expression.Compile().Invoke();

            Assert.IsNotNull(result);
        }
    }
}
