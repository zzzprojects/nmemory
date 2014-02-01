// -----------------------------------------------------------------------------------
// <copyright file="KeyInfoHelperFixture.cs" company="NMemory Team">
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

namespace NMemory.Test.Indexes
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Indexes;
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class KeyInfoHelperFixture
    {
        [TestMethod]
        public void TupleKeyInfoHelper_Parse()
        {
            TupleKeyInfoHelper builder = new TupleKeyInfoHelper(typeof(Tuple<int, string>));

            Expression<Func<Group, Tuple<int, string>>> selector = 
                (Group m) => new Tuple<int, string>(m.Id, m.Name);

            MemberInfo[] members = null;
            bool res = builder.TryParseKeySelectorExpression(selector.Body, true, out members);

            Assert.AreEqual(true, res);
            Assert.IsNotNull(members);
            Assert.AreEqual(2, members.Length);
            Assert.AreEqual("Id", members[0].Name);
            Assert.AreEqual("Name", members[1].Name);
        }

        [TestMethod]
        public void TupleKeyInfoHelper_ParseCreate()
        {
            TupleKeyInfoHelper builder = new TupleKeyInfoHelper(typeof(Tuple<int, string>));

            Expression<Func<Group, Tuple<int, string>>> selector =
                (Group m) => Tuple.Create(m.Id, m.Name);

            MemberInfo[] members = null;
            bool res = builder.TryParseKeySelectorExpression(selector.Body, true, out members);

            Assert.AreEqual(true, res);
            Assert.IsNotNull(members);
            Assert.AreEqual(2, members.Length);
            Assert.AreEqual("Id", members[0].Name);
            Assert.AreEqual("Name", members[1].Name);
        }

        [TestMethod]
        public void TupleKeyInfoHelper_ParseLarge()
        {
            TupleKeyInfoHelper builder = new TupleKeyInfoHelper(typeof(Tuple<int, string, int, string, int, string, int, Tuple<string, int>>));

            Expression<Func<Group, Tuple<int, string, int, string, int, string, int, Tuple<string, int>>>> 
                selector = (Group m) =>
                    new Tuple<int, string, int, string, int, string, int, Tuple<string, int>>(
                        m.Id, 
                        m.Name, 
                        m.Id, 
                        m.Name, 
                        m.Id, 
                        m.Name,
                        m.Id, 
                        new Tuple<string, int>(
                            m.Name,
                            m.Id));
                        
            MemberInfo[] members = null;
            bool res = builder.TryParseKeySelectorExpression(selector.Body, true, out members);

            Assert.AreEqual(true, res);
            Assert.IsNotNull(members);
            Assert.AreEqual(9, members.Length);

            for (int i = 0; i < 9; i++)
            {
                string expect = i % 2 == 0 ? "Id" : "Name";
                Assert.AreEqual(expect, members[i].Name);
            }
        }

        [TestMethod]
        public void TupleKeyInfoHelper_MemberCount()
        {
            TupleKeyInfoHelper builder = new TupleKeyInfoHelper(typeof(Tuple<int, string>));

            int memberCount = builder.GetMemberCount();

            Assert.AreEqual(2, memberCount);
        }

        [TestMethod]
        public void TupleKeyInfoHelper_MemberCountLarge()
        {
            TupleKeyInfoHelper builder =
                new TupleKeyInfoHelper(typeof(Tuple<int, int, int, int, int, int, int, Tuple<int, int>>));

            int memberCount = builder.GetMemberCount();

            Assert.AreEqual(9, memberCount);
        }

        [TestMethod]
        public void TupleKeyInfoHelper_KeyFactory()
        {
            TupleKeyInfoHelper builder = new TupleKeyInfoHelper(typeof(Tuple<int, string>));

            Expression factory = builder.CreateKeyFactoryExpression(
                Expression.Constant(1, typeof(int)), 
                Expression.Constant("2", typeof(string)));

            var result = Expression.Lambda(factory).Compile().DynamicInvoke() as Tuple<int, string>;

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Item1);
            Assert.AreEqual("2", result.Item2);
        }

        [TestMethod]
        public void TupleKeyInfoHelper_KeyFactoryLarge()
        {
            TupleKeyInfoHelper builder =
                new TupleKeyInfoHelper(typeof(Tuple<int, int, int, int, int, int, int, Tuple<int, int>>));

            Expression factory = builder.CreateKeyFactoryExpression(
                Enumerable
                    .Range(1, 9)
                    .Select(x => Expression.Constant(x, typeof(int)))
                    .ToArray());

            var result = Expression.Lambda(factory).Compile().DynamicInvoke()
                as Tuple<int, int, int, int, int, int, int, Tuple<int, int>>;

            Assert.IsNotNull(result);

            Assert.AreEqual(1, result.Item1);
            Assert.AreEqual(2, result.Item2);
            Assert.AreEqual(8, result.Rest.Item1);
            Assert.AreEqual(9, result.Rest.Item2);
        }

        [TestMethod]
        public void TupleKeyInfoHelper_KeyMemberSelector()
        {
            TupleKeyInfoHelper builder = new TupleKeyInfoHelper(typeof(Tuple<int, string>));

            Expression source = Expression.Constant(Tuple.Create(1, "2"));
            Expression selector1 = builder.CreateKeyMemberSelectorExpression(source, 0);
            Expression selector2 = builder.CreateKeyMemberSelectorExpression(source, 1);

            int result1 = (int)Expression.Lambda(selector1).Compile().DynamicInvoke();
            string result2 = (string)Expression.Lambda(selector2).Compile().DynamicInvoke();

            Assert.AreEqual(1, result1);
            Assert.AreEqual("2", result2);
        }

        [TestMethod]
        public void TupleKeyInfoHelper_KeyMemberSelectorLarge()
        {
            var tuple = new Tuple<int, int, int, int, int, int, int, Tuple<int, int>>(
                1, 2, 3, 4, 5, 6, 7, new Tuple<int, int>(8, 9));

            TupleKeyInfoHelper builder = new TupleKeyInfoHelper(tuple.GetType());

            Expression source = Expression.Constant(tuple);
            Expression selector = builder.CreateKeyMemberSelectorExpression(source, 7);

            int result = (int)Expression.Lambda(selector).Compile().DynamicInvoke();

            Assert.AreEqual(8, result);
        }

        [TestMethod]
        public void TupleKeyInfoHelper_KeyMemberSelectorLarge2()
        {
            var tuple = 
                new Tuple<int, int, int, int, int, int, int, 
                    Tuple<int, int, int, int, int, int, int, 
                        Tuple<int, int>>>(
                1, 2, 3, 4, 5, 6, 7, 
                    new Tuple<int, int, int, int, int, int, int, Tuple<int, int>>(
                        8, 9, 10, 11, 12, 13, 14, Tuple.Create(15, 16)));

            TupleKeyInfoHelper builder = new TupleKeyInfoHelper(tuple.GetType());

            Expression source = Expression.Constant(tuple);
            Expression selector = builder.CreateKeyMemberSelectorExpression(source, 15);

            int result = (int)Expression.Lambda(selector).Compile().DynamicInvoke();

            Assert.AreEqual(16, result);
        }

        [TestMethod]
        public void PrimitiveKeyInfoHelper_KeyFactory()
        {
            PrimitiveKeyInfoHelper builder = new PrimitiveKeyInfoHelper(typeof(int));

            Expression factory = builder.CreateKeyFactoryExpression(
                Expression.Constant(1, typeof(int)));

            int result = (int)Expression.Lambda(factory).Compile().DynamicInvoke();

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void PrimitiveKeyInfoHelper_KeyMemberSelector()
        {
            PrimitiveKeyInfoHelper builder = new PrimitiveKeyInfoHelper(typeof(int));
            Expression source = Expression.Constant(1, typeof(int));

            Expression selector = builder.CreateKeyMemberSelectorExpression(source, 0);

            int result = (int)Expression.Lambda(selector).Compile().DynamicInvoke();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoHelper_KeyFactory()
        {
            var key = new { Key1 = 1, Key2 = "2" };
            AnonymousTypeKeyInfoHelper builder = new AnonymousTypeKeyInfoHelper(key.GetType());
           
            Expression factory = builder.CreateKeyFactoryExpression(
                Expression.Constant(1, typeof(int)),
                Expression.Constant("2", typeof(string)));

            dynamic result = Expression.Lambda(factory).Compile().DynamicInvoke();

            Assert.AreEqual(1, result.Key1);
            Assert.AreEqual("2", result.Key2);
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoHelper_KeyMemberSelector()
        {
            var key = new { Key1 = 1, Key2 = "2" };
            AnonymousTypeKeyInfoHelper builder = new AnonymousTypeKeyInfoHelper(key.GetType());

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
