using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Indexes;
using System.Linq.Expressions;

namespace NMemory.Test
{
    [TestClass]
    public class KeyInfoExpressionBuilderFixture
    {
        [TestMethod]
        public void TupleKeyInfoExpressionBuilderFactory()
        {
            Type tupleType = typeof(Tuple<int, string>);

            TupleKeyInfoExpressionBuilder builder = new TupleKeyInfoExpressionBuilder(tupleType);

            Expression factory = builder.CreateKeyFactoryExpression(
                Expression.Constant(1, typeof(int)), 
                Expression.Constant("2", typeof(string)));

            Tuple<int, string> result = Expression.Lambda(factory).Compile().DynamicInvoke() as Tuple<int, string>;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Item1);
            Assert.AreEqual("2", result.Item2);
        }

        [TestMethod]
        public void TupleKeyInfoExpressionBuilderSelector()
        {
            Type tupleType = typeof(Tuple<int, string>);
            Expression source = Expression.Constant(Tuple.Create(1, "2"));

            TupleKeyInfoExpressionBuilder builder = new TupleKeyInfoExpressionBuilder(tupleType);

            Expression selector1 = builder.CreateKeyMemberSelectorExpression(source, 0);
            Expression selector2 = builder.CreateKeyMemberSelectorExpression(source, 1);

            int result1 = (int)Expression.Lambda(selector1).Compile().DynamicInvoke();
            string result2 = (string)Expression.Lambda(selector2).Compile().DynamicInvoke();

            Assert.AreEqual(1, result1);
            Assert.AreEqual("2", result2);
        }
    }
}
