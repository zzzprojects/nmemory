// -----------------------------------------------------------------------------------
// <copyright file="ConstraintFixture.cs" company="NMemory Team">
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

namespace NMemory.Test
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Exceptions;
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class ConstraintFixture
    {
        [TestMethod]
        [ExpectedException(typeof(ConstraintException))]
        public void NCharContraintViolation()
        {
            TestDatabase db = new TestDatabase(createNcharContraintForGroup: true);

            db.Groups.Insert(new Group { Name = "Too long name" });
        }

        [TestMethod]
        public void NCharContraintCompletion()
        {
            TestDatabase db = new TestDatabase(createNcharContraintForGroup: true);

            Group group = new Group { Id = 1, Name = "wat" };
            db.Groups.Insert(group);

            Assert.AreEqual("wat ", db.Groups.Single().Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ConstraintException))]
        public void NotNullableConstraintCompletion()
        {
            TestDatabase db = new TestDatabase(createNotNullConstraint: true);

            Group group = new Group { Id = 1};
            db.Groups.Insert(group);
        }
    }
}
