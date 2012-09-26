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
        private IExecutionPlan<IEnumerable<TEntity>> plan;
        private bool storeCompilation;

        #region Ctor

        internal TableQuery(IDatabase database, Expression expression, IQueryProvider provider) 
            : base(database, expression, provider)
        {
            this.storeCompilation = false;
        }

        public TableQuery(IDatabase database, Expression expression)
            : base(database, expression)
        {
            this.storeCompilation = false;
        }

        public TableQuery(IDatabase database, Expression expression, bool storeCompilation)
            : base(database, expression)
        {
            this.storeCompilation = storeCompilation;
        }

        protected TableQuery(IDatabase database)
            :base(database)
        {
            this.storeCompilation = false;
        }

        protected TableQuery(IDatabase database, bool storeCompilation)
            : base(database)
        {
            this.storeCompilation = storeCompilation;
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
            IExecutionPlan<IEnumerable<TEntity>> planToExecute = null;

            if (this.storeCompilation)
            {
                this.Compile();
                planToExecute = this.plan;
                
            }
            else
            {
                planToExecute = this.CompilePlan();
            }

            using (var ctx = Transaction.EnsureTransaction(ref transaction, this.Database))
            {
                IExecutionContext context = new ExecutionContext(this.Database, transaction);
                IEnumerator<TEntity> result = this.Database.DatabaseEngine.Executor.Execute(planToExecute, context);

                ctx.Complete();

                return result;
            }
        }

        public void Compile()
        {
            if (this.plan != null || !this.storeCompilation)
            {
                return;
            }

            this.plan = this.CompilePlan();
        }

        private IExecutionPlan<IEnumerable<TEntity>> CompilePlan()
        {
            Expression expression = ((IQueryable)this).Expression;

            return this.Database.DatabaseEngine.Compiler.Compile<IEnumerable<TEntity>>(expression);
        }

    }
    
}
