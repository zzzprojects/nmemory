// -----------------------------------------------------------------------------------
// <copyright file="KeyInfoFixture.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Indexes;
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class KeyInfoFixture
    {
        [TestMethod]
        public void PrimitiveKeyInfoCreation()
        {
            var keyInfo = PrimitiveKeyInfo.Create((Group g) => g.Id);

            Assert.AreEqual(1, keyInfo.EntityKeyMembers.Length);
        }

        [TestMethod]
        public void PrimitiveKeyInfoIsEmpty()
        {
            var keyInfo = PrimitiveKeyInfo.Create((Member g) => g.GroupId);

            Assert.IsTrue(keyInfo.IsEmptyKey(null));
            Assert.IsFalse(keyInfo.IsEmptyKey(1));
        }


        [TestMethod]
        public void PrimitiveKeyInfoComparer()
        {
            var keyInfo = PrimitiveKeyInfo.Create((Member g) => g.GroupId);
            var comparer = keyInfo.KeyComparer;

            Assert.AreEqual(0, comparer.Compare(null, null));
            Assert.AreEqual(-1, comparer.Compare(null, 1));
            Assert.AreEqual(1, comparer.Compare(1, null));

            Assert.AreEqual(0, comparer.Compare(1, 1));
            Assert.AreEqual(-1, comparer.Compare(0, 1));
            Assert.AreEqual(1, comparer.Compare(1, 0));
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoCreation()
        {
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { m.GroupId, m.GroupId2 });

            Assert.AreEqual(2, keyInfo.EntityKeyMembers.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AnonymousTypeKeyInfoInvalidCreation()
        {
            // Casting must not be used in the selector expression
            AnonymousTypeKeyInfo.Create((Member m) => new { Id1 = m.GroupId, Id2 = (long)m.GroupId2 });
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoIsEmpty()
        {
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { m.GroupId, m.GroupId2 });

            Assert.IsTrue(keyInfo.IsEmptyKey(new { GroupId = (int?)null, GroupId2 = 1 }));
            Assert.IsFalse(keyInfo.IsEmptyKey(new { GroupId = (int?)1, GroupId2 = 2 }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AnonymousTypeKeyInfoComparerNullThrowsException()
        {
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { m.GroupId, m.GroupId2 });
            var comparer = keyInfo.KeyComparer;

            comparer.Compare(null, null);
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoComparer()
        {
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { m.GroupId, m.GroupId2 });
            var comparer = keyInfo.KeyComparer;

            Assert.AreEqual(0, comparer.Compare(
                new { GroupId = (int?)null, GroupId2 = 1 }, 
                new { GroupId = (int?)null, GroupId2 = 1 }));

            Assert.AreEqual(-1, comparer.Compare(
                new { GroupId = (int?)null, GroupId2 = 1 }, 
                new { GroupId = (int?)1, GroupId2 = 1 }));

            Assert.AreEqual(1, comparer.Compare(
                new { GroupId = (int?)1, GroupId2 = 1 }, 
                new { GroupId = (int?)null, GroupId2 = 1 }));



            Assert.AreEqual(-1, comparer.Compare(
                new { GroupId = (int?)null, GroupId2 = 1 },
                new { GroupId = (int?)null, GroupId2 = 2 }));

            Assert.AreEqual(1, comparer.Compare(
                new { GroupId = (int?)null, GroupId2 = 2 },
                new { GroupId = (int?)null, GroupId2 = 1 }));
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoComparerDescending()
        {
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { m.GroupId, m.GroupId2 }, SortOrder.Descending, SortOrder.Descending);
            var comparer = keyInfo.KeyComparer;

            Assert.AreEqual(0, comparer.Compare(
                new { GroupId = (int?)null, GroupId2 = 1 },
                new { GroupId = (int?)null, GroupId2 = 1 }));

            Assert.AreEqual(1, comparer.Compare(
                new { GroupId = (int?)null, GroupId2 = 1 },
                new { GroupId = (int?)1, GroupId2 = 1 }));

            Assert.AreEqual(-1, comparer.Compare(
                new { GroupId = (int?)1, GroupId2 = 1 },
                new { GroupId = (int?)null, GroupId2 = 1 }));


            Assert.AreEqual(1, comparer.Compare(
                new { GroupId = (int?)null, GroupId2 = 1 },
                new { GroupId = (int?)null, GroupId2 = 2 }));

            Assert.AreEqual(-1, comparer.Compare(
                new { GroupId = (int?)1, GroupId2 = 2 },
                new { GroupId = (int?)1, GroupId2 = 1 }));
        }

        [TestMethod]
        public void TupleKeyInfoCreation()
        {
            var keyInfo = TupleKeyInfo.Create((Member m) => Tuple.Create(m.GroupId, m.GroupId2));

            Assert.AreEqual(2, keyInfo.EntityKeyMembers.Length);
        }

        [TestMethod]
        public void TupleKeyInfoCreation2()
        {
            var keyInfo = TupleKeyInfo.Create((Member m) => new Tuple<int?, int>(m.GroupId, m.GroupId2));

            Assert.AreEqual(2, keyInfo.EntityKeyMembers.Length);
        }

        [TestMethod]
        public void TupleKeyInfoIsEmpty()
        {
            var keyInfo = TupleKeyInfo.Create((Member m) => Tuple.Create(m.GroupId, m.GroupId2));

            Assert.AreEqual(false, keyInfo.IsEmptyKey(Tuple.Create((int?)1, 1)));
            Assert.AreEqual(true, keyInfo.IsEmptyKey(Tuple.Create((int?)null, 1)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TupleKeyInfoComparerNullThrowsException()
        {
            var keyInfo = TupleKeyInfo.Create((Member m) => Tuple.Create(m.GroupId, m.GroupId2));
            var comparer = keyInfo.KeyComparer;

            comparer.Compare(null, null);
        }

        [TestMethod]
        public void TupleKeyInfoComparer()
        {
            var keyInfo = TupleKeyInfo.Create((Member m) => Tuple.Create(m.GroupId, m.GroupId2));
            var comparer = keyInfo.KeyComparer;

            Assert.AreEqual(0, comparer.Compare(
                Tuple.Create((int?)null, 1),
                Tuple.Create((int?)null, 1)));

            Assert.AreEqual(-1, comparer.Compare(
                Tuple.Create((int?)null, 1),
                Tuple.Create((int?)1, 1)));

            Assert.AreEqual(1, comparer.Compare(
                Tuple.Create((int?)1, 1),
                Tuple.Create((int?)null, 1)));

            Assert.AreEqual(0, comparer.Compare(
               Tuple.Create((int?)1, 1),
               Tuple.Create((int?)1, 1)));

            Assert.AreEqual(-1, comparer.Compare(
                Tuple.Create((int?)1, 1),
                Tuple.Create((int?)2, 1)));

            Assert.AreEqual(1, comparer.Compare(
                Tuple.Create((int?)2, 1),
                Tuple.Create((int?)1, 1)));


            Assert.AreEqual(-1, comparer.Compare(
                Tuple.Create((int?)1, 1),
                Tuple.Create((int?)1, 2)));

            Assert.AreEqual(1, comparer.Compare(
                Tuple.Create((int?)1, 2),
                Tuple.Create((int?)1, 1)));

        }

        [TestMethod]
        public void TupleKeyInfoComparerDescending()
        {
            var keyInfo = TupleKeyInfo.Create((Member m) => Tuple.Create(m.GroupId, m.GroupId2), SortOrder.Descending, SortOrder.Descending);
            var comparer = keyInfo.KeyComparer;

            Assert.AreEqual(0, comparer.Compare(
                Tuple.Create((int?)null, 1), 
                Tuple.Create((int?)null, 1)));

            Assert.AreEqual(1, comparer.Compare(
                Tuple.Create((int?)null, 1),
                Tuple.Create((int?)1, 1)));

            Assert.AreEqual(-1, comparer.Compare(
                Tuple.Create((int?)1, 1),
                Tuple.Create((int?)null, 1)));


            Assert.AreEqual(0, comparer.Compare(
               Tuple.Create((int?)1, 1),
               Tuple.Create((int?)1, 1)));

            Assert.AreEqual(1, comparer.Compare(
                Tuple.Create((int?)1, 1),
                Tuple.Create((int?)2, 1)));

            Assert.AreEqual(-1, comparer.Compare(
                Tuple.Create((int?)2, 1),
                Tuple.Create((int?)1, 1)));


            Assert.AreEqual(1, comparer.Compare(
                Tuple.Create((int?)1, 1),
                Tuple.Create((int?)1, 2)));

            Assert.AreEqual(-1, comparer.Compare(
                Tuple.Create((int?)1, 2),
                Tuple.Create((int?)1, 1)));

        }

        
    }
}
