// ----------------------------------------------------------------------------------
// <copyright file="TableQuery`1.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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

namespace NMemory.Linq
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Execution;
    using NMemory.Modularity;
    using NMemory.Transactions;

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
            return GetEnumerator(null, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public IEnumerator<TEntity> GetEnumerator(IDictionary<string, object> parameters, Transaction transaction)
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

            using (TransactionContext transactionContext = Transaction.EnsureTransaction(ref transaction, this.Database))
            {
                IExecutionContext executionContext = new ExecutionContext(this.Database, transaction, parameters);
                IEnumerator<TEntity> result = this.Database.DatabaseEngine.Executor.Execute(planToExecute, executionContext);

                transactionContext.Complete();

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
