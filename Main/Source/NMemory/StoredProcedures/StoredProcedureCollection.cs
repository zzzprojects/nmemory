// ----------------------------------------------------------------------------------
// <copyright file="StoredProcedureCollection.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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
// ----------------------------------------------------------------------------------

namespace NMemory.StoredProcedures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NMemory.Linq;

    public class StoredProcedureCollection
    {
        private WeakReference database;
        private List<object> storedProcedures;

        internal StoredProcedureCollection(Database database)
        {
            if (database == null)
            {
                throw new ArgumentNullException();
            }

            this.database = new WeakReference(database);
            this.storedProcedures = new List<object>();
        }

        private Database Database
        {
            get { return this.database.Target as Database; }
        }

        public IStoredProcedure<T> Create<T>(IQueryable<T> query)
        {
            return this.Create<T>(query, false);
        }

        public IStoredProcedure<T> Create<T>(IQueryable<T> query, bool precompiled)
        {
            this.ValidateQuery<T>(query);

            StoredProcedure<T> procedure = new StoredProcedure<T>(query, precompiled);

            this.storedProcedures.Add(procedure);
            return procedure;
        }

        private void ValidateQuery<T>(IQueryable<T> query)
        {
            ITableQuery tableQuery = query as ITableQuery;

            if (tableQuery == null)
            {
                throw new ArgumentException("Query is not an NMemory query.", "query");
            }

            if (tableQuery.Database != this.Database)
            {
                throw new ArgumentException("Query does not belong to this database.", "query");
            }
        }

    }
}
