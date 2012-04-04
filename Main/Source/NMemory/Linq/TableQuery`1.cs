using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common.Visitors;
using NMemory.Execution;
using NMemory.Transactions;

namespace NMemory.Linq
{
    public class TableQuery<TEntity> : TableQuery, IQueryable<TEntity>
    {

        #region Ctor

        internal TableQuery(Database database, Expression expression, IQueryProvider provider) 
            : base(database, expression, provider)
        {
           
        }

        public TableQuery(Database database, Expression expression)
            : base(database, expression)
        {
            
        }

        protected TableQuery(Database database)
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

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            using (var tran = Transaction.EnsureTransaction(this.Database))
            {
                var tables = TableSearchVisitor.FindTables(((IQueryable)this).Expression);
                var compiledQuery = this.Database.Compiler.Compile<IEnumerable<TEntity>>(((IQueryable)this).Expression);

                var context = new ExecutionContext(tables);
                var result = this.Database.Executor.Execute(compiledQuery, context);

                tran.Complete();

                return result;
            }
        }
    }
    
}
