using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common.Visitors;
using NMemory.Execution;
using NMemory.Transactions;
using NMemory.Modularity;

namespace NMemory.Linq
{
    public class TableQuery<TEntity> : TableQuery, IQueryable<TEntity>
    {
        #region Ctor

        internal TableQuery(IDatabase database, Expression expression, IQueryProvider provider) 
            : base(database, expression, provider)
        {
           
        }

        public TableQuery(IDatabase database, Expression expression)
            : base(database, expression)
        {
            
        }

        protected TableQuery(IDatabase database)
            :base(database)
        {
            
        }

        #endregion

        public override Type EntityType
        {
            get { return typeof(TEntity); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return GetEnumerator(Transaction.TryGetAmbientEnlistedTransaction());
        }

        public IEnumerator<TEntity> GetEnumerator(Transaction transaction)
        {
            using (var ctx = Transaction.EnsureTransaction(ref transaction, this.Database))
            {
                // TODO: Ensure transaction

                var tables = TableSearchVisitor.FindTables(((IQueryable)this).Expression);
                var compiledQuery = this.Database.DatabaseEngine.Compiler.Compile<IEnumerable<TEntity>>(((IQueryable)this).Expression);

                var context = new ExecutionContext(transaction, tables);
                var result = this.Database.DatabaseEngine.Executor.Execute(compiledQuery, context);

                ctx.Complete();

                return result;
            }
        }

    }
    
}
