using System;
using System.Collections.Generic;
using System.Linq;
using NMemory.Linq;

namespace NMemory.StoredProcedures
{
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
            this.ValidateQuery<T>(query);

            StoredProcedure<T> procedure = new StoredProcedure<T>(query);

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
