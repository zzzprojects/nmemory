// -----------------------------------------------------------------------------------
// <copyright file="StoredProcedureFixture.cs" company="NMemory Team">
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

namespace NMemory.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NMemory.Exceptions;
    using NMemory.StoredProcedures;
    using NMemory.Test.Environment.Data;

    [TestClass]
    public class StoredProcedureFixture
    {
        [TestMethod]
        public void StoredProcedure_ParameterDescription()
        {
            TestDatabase database = new TestDatabase();

            IQueryable<Group> query = 
                database.Groups.Where(g =>
                    g.Id > new Parameter<int>("param1") + new Parameter<long?>("param2"));

            var procedure = database.StoredProcedures.Create(query);

            Assert.AreEqual(procedure.Parameters.Count, 2);
            Assert.IsTrue(procedure.Parameters.Any(p => 
                p.Name == "param1" && p.Type == typeof(int)));
            Assert.IsTrue(procedure.Parameters.Any(p =>
                p.Name == "param2" && p.Type == typeof(long?)));
        }

        [TestMethod]
        public void StoredProcedure_InternalParameter()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            IQueryable<Group> query = 
                database.Groups.Where(g => g.Id > new Parameter<int>("param1"));

            var procedure = database.StoredProcedures.Create(query);

            var result = procedure
                .Execute(new Dictionary<string, object> { { "param1", 1 } })
                .ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(g => g.Id > 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterException))]
        public void StoredProcedure_IncompatibleType1()
        {
            TestDatabase database = new TestDatabase();

            IQueryable<Group> query =
                database.Groups.Where(g =>
                    g.Id == new Parameter<int>("param1"));

            var procedure = database.StoredProcedures.Create(query);

            var result = procedure
                .Execute(
                    new Dictionary<string, object> { { "param1", null } })
                .ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterException))]
        public void StoredProcedure_IncompatibleType2()
        {
            TestDatabase database = new TestDatabase();

            IQueryable<Group> query =
                database.Groups.Where(g =>
                    g.Id == new Parameter<int>("param1"));

            var procedure = database.StoredProcedures.Create(query);

            var result = procedure
                .Execute(
                    new Dictionary<string, object> { { "param1", "" } })
                .ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterException))]
        public void StoredProcedure_MissingParameter()
        {
            TestDatabase database = new TestDatabase();

            IQueryable<Group> query =
                database.Groups.Where(g =>
                    g.Id == new Parameter<int>("param1"));

            var procedure = database.StoredProcedures.Create(query);

            var result = procedure
                .Execute(
                    new Dictionary<string, object> { { "param1", "" } })
                .ToList();
        }

        [TestMethod]
        public void SharedStoredProcedure()
        {
            TestDatabase database = new TestDatabase();

            database.Groups.Insert(new Group { Name = "Group 1" });
            database.Groups.Insert(new Group { Name = "Group 2" });
            database.Groups.Insert(new Group { Name = "Group 3" });

            var procedure = new SharedStoredProcedure<TestDatabase, Group>(
                d => d.Groups.Where(g => g.Id > 1));

            var result = procedure.Execute(database, null).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(g => g.Id > 1));
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterException))]
        public void SharedStoredProcedure_IncompatibleType1()
        {
            TestDatabase database = new TestDatabase();

            var procedure = new SharedStoredProcedure<TestDatabase, Group>(
               d => d.Groups.Where(g => g.Id == new Parameter<int>("param1")));

            var result = procedure
                .Execute(
                    database,
                    new Dictionary<string, object> { { "param1", null } })
                .ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterException))]
        public void SharedStoredProcedure_IncompatibleType2()
        {
            TestDatabase database = new TestDatabase();

            var procedure = new SharedStoredProcedure<TestDatabase, Group>(
               d => d.Groups.Where(g => g.Id == new Parameter<int>("param1")));

            var result = procedure
                .Execute(
                    database,
                    new Dictionary<string, object> { { "param1", "" } })
                .ToList();
        }

        [TestMethod]
        [ExpectedException(typeof(ParameterException))]
        public void SharedStoredProcedure_MissingParameter()
        {
            TestDatabase database = new TestDatabase();

            var procedure = new SharedStoredProcedure<TestDatabase, Group>(
               d => d.Groups.Where(g => g.Id == new Parameter<int>("param1")));

            var result = procedure
                .Execute(
                    database,
                    new Dictionary<string, object> { { "param1", "" } })
                .ToList();
        }
    }
}
