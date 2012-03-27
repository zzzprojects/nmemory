using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Modularity;
using NMemory.Transactions;

namespace NMemory.Linq
{
    public class TableQuery<TEntity> : IQueryable<TEntity>, ITableQuery
    {
        private Expression expression;
        private Database database;
        private IQueryProvider provider;

        #region Ctor

        internal TableQuery(Database database, Expression expression, IQueryProvider provider)
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

        internal TableQuery(Database database, Expression expression)
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

        internal TableQuery(Database database)
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

        public Database Database
        {
            get { return this.database; }
        }

        public Type EntityType
        {
            get { return typeof(TEntity); }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(TEntity); }
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
            return this.GetEnumerator();
        }

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            IQueryEnumeratorFactory enumeratorFactory = this.Database.Executor.CreateQueryEnumeratorFactory();

            return enumeratorFactory.Create<TEntity>(this, this.EnsureTransaction());
        }

        protected bool CheckTransaction()
        {
            if (Transaction.Current != null || Transaction.TryEnlistOnTransient())
            {
                if (Transaction.Current.Aborted)
                {
                    throw new System.Transactions.TransactionAbortedException();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected TransactionWrapper EnsureTransaction()
        {
            if (this.CheckTransaction())
            {
                // Transaction is present, create empty wrapper
                return new TransactionWrapper(null);
            }
            else
            {
                // There is no transaction, create a local transaction and wrap it
                return new TransactionWrapper(Transaction.CreateLocal());
            }
        }
    }
    
}
