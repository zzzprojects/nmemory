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
        public void AnonymousTypeKeyInfoCreation()
        {
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { m.GroupId, m.GroupId2 });

            Assert.AreEqual(2, keyInfo.EntityKeyMembers.Length);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AnonymousTypeKeyInfoInvalidCreation()
        {
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { Id1 = m.GroupId, Id2 = (long)m.GroupId2 });

            Assert.AreEqual(2, keyInfo.EntityKeyMembers.Length);
        }

        [TestMethod]
        public void AnonymousTypeKeyInfoIsEmpty()
        {
            var keyInfo = AnonymousTypeKeyInfo.Create((Member m) => new { m.GroupId, m.GroupId2 });

            Assert.IsTrue(keyInfo.IsEmptyKey(new { GroupId = (int?)null, GroupId2 = 1 }));

            Assert.IsFalse(keyInfo.IsEmptyKey(new { GroupId = (int?)1, GroupId2 = 2 }));
        }
    }
}
