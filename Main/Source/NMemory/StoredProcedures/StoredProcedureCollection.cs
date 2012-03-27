using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Linq;
using System.Collections;
using System.Linq.Expressions;

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

            this.database = new WeakReference(this.database);
            this.storedProcedures = new List<object>();
        }

        ////public Func<TParam1, IEnumerable<TEntity>> Create<TEntity, TParam1>(IQueryable<TEntity> query)
        ////{
        ////    var sp = new StoredProcedure<TEntity, TParam1>(query, this.Database);

        ////    this.storedProcedures.Add(sp);

        ////    return sp.Procedure;
        ////}


        ////public Func<TParam1, IEnumerable<TEntity>> CreateStoredProcedure<TEntity, TParam1>(IQueryable<TEntity> query)
        ////{
        ////    var sp = new StoredProcedure<TEntity, TParam1>(query, this.Database);

        ////    this.storedProcedures.Add(sp);

        ////    return sp.Procedure;
        ////}

        ////public Func<Dictionary<string, object>, IEnumerable<TEntity>> CreateStoredProcedure<TEntity>(IQueryable<TEntity> query)
        ////{
        ////    var sp = new StoredProcedure<TEntity>(query, this.Database);

        ////    this.storedProcedures.Add(sp);

        ////    return sp.Procedure;
        ////}

        ////public Func<Dictionary<string, object>, IEnumerable> CreateStoredProcedure(Expression expression)
        ////{
        ////    var sp = new StoredProcedure(expression, this.Database);

        ////    this.storedProcedures.Add(sp);

        ////    return sp.Procedure;
        ////}

        private Database Database
        {
            get
            {
                return this.database.Target as Database;
            }
        }
    }
}
