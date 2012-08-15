using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Transactions;
using NMemory.Modularity;

namespace NMemory.Linq
{
    public abstract class TableQuery : IQueryable, ITableQuery
    {
        private Expression expression;
        private IDatabase database;
        private IQueryProvider provider;

        #region Ctor

        internal TableQuery(IDatabase database, Expression expression, IQueryProvider provider)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            this.database = database;
            this.expression = expression;
            this.provider = provider;
        }

        internal TableQuery(IDatabase database, Expression expression)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            this.database = database;
            this.expression = expression;
            this.provider = new TableQueryProvider(database);
        }

        internal TableQuery(IDatabase database)
        {
            if (database == null)
            {
                throw new ArgumentNullException("database");
            }

            this.database = database;
            this.expression = Expression.Constant(this);
            this.provider = new TableQueryProvider(database);
        }

        #endregion

        public IDatabase Database
        {
            get { return this.database; }
        }

        public abstract Type EntityType
        {
            get;
        }

        Type IQueryable.ElementType
        {
            get { return this.EntityType; }
        }

        Expression IQueryable.Expression
        {
            get { return this.expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this.provider; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return null;
        }
    }
}
