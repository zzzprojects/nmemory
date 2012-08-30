using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Test.Data;
using NMemory.Indexes;

namespace NMemory.Test
{
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
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { m.GroupId, m.GroupId2 }, SortOrder.Descending, SortOrder.Ascending);
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



            Assert.AreEqual(-1, comparer.Compare(
                new { GroupId = (int?)null, GroupId2 = 1 },
                new { GroupId = (int?)null, GroupId2 = 2 }));

            Assert.AreEqual(1, comparer.Compare(
                new { GroupId = (int?)1, GroupId2 = 2 },
                new { GroupId = (int?)1, GroupId2 = 1 }));
        }


    }
}
