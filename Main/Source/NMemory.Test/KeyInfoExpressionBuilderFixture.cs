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
            TupleKeyInfoExpressionBuilder builder = new TupleKeyInfoExpressionBuilder(typeof(Tuple<int, string>));

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
            TupleKeyInfoExpressionBuilder builder = new TupleKeyInfoExpressionBuilder(typeof(Tuple<int, string>));

            Expression source = Expression.Constant(Tuple.Create(1, "2"));
            Expression selector1 = builder.CreateKeyMemberSelectorExpression(source, 0);
            Expression selector2 = builder.CreateKeyMemberSelectorExpression(source, 1);

            int result1 = (int)Expression.Lambda(selector1).Compile().DynamicInvoke();
            string result2 = (string)Expression.Lambda(selector2).Compile().DynamicInvoke();

            Assert.AreEqual(1, result1);
            Assert.AreEqual("2", result2);
        }

        [TestMethod]
        public void PrimitiveKeyInfoExpressionBuilderFactory()
        {
            PrimitiveKeyInfoExpressionBuilder builder = new PrimitiveKeyInfoExpressionBuilder(typeof(int));

            Expression factory = builder.CreateKeyFactoryExpression(
                Expression.Constant(1, typeof(int)));

            int result = (int)Expression.Lambda(factory).Compile().DynamicInvoke();

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void PrimitiveKeyInfoExpressionBuilderSelector()
        {
            PrimitiveKeyInfoExpressionBuilder builder = new PrimitiveKeyInfoExpressionBuilder(typeof(int));
            Expression source = Expression.Constant(1, typeof(int));

            Expression selector = builder.CreateKeyMemberSelectorExpression(source, 0);

            int result = (int)Expression.Lambda(selector).Compile().DynamicInvoke();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoExpressionBuilderFactory()
        {
            var key = new { Key1 = 1, Key2 = "2" };
            AnonymousTypeKeyInfoExpressionBuilder builder = new AnonymousTypeKeyInfoExpressionBuilder(key.GetType());
           
            Expression factory = builder.CreateKeyFactoryExpression(
                Expression.Constant(1, typeof(int)),
                Expression.Constant("2", typeof(string)));

            dynamic result = Expression.Lambda(factory).Compile().DynamicInvoke();

            Assert.AreEqual(1, result.Key1);
            Assert.AreEqual("2", result.Key2);
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoExpressionBuilderSelector()
        {
            var key = new { Key1 = 1, Key2 = "2" };
            AnonymousTypeKeyInfoExpressionBuilder builder = new AnonymousTypeKeyInfoExpressionBuilder(key.GetType());

            Expression source = Expression.Constant(key);
            Expression selector1 = builder.CreateKeyMemberSelectorExpression(source, 0);
            Expression selector2 = builder.CreateKeyMemberSelectorExpression(source, 1);

            int result1 = (int)Expression.Lambda(selector1).Compile().DynamicInvoke();
            string result2 = (string)Expression.Lambda(selector2).Compile().DynamicInvoke();

            Assert.AreEqual(1, result1);
            Assert.AreEqual("2", result2);
        }
    }
}
