using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common.Visitors;
using NMemory.Execution;
using NMemory.Tables;
using NMemory.Transactions;
using NMemory.Modularity;

namespace NMemory.Linq
{
	public class TableQueryProvider : IQueryProvider
	{
        private IDatabase database;

		public TableQueryProvider(IDatabase database)
		{
            this.database = database;
		}

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TableQuery<TElement>(database, expression, this);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            Transaction transaction = Transaction.TryGetAmbientEnlistedTransaction();

            using (var tran = Transaction.EnsureTransaction(ref transaction, this.database))
            {
                IExecutionPlan<TResult> plan = this.database.DatabaseEngine.Compiler.Compile<TResult>(expression);
                IExecutionContext context = new ExecutionContext(this.database, transaction);

                TResult result = plan.Execute(context);

                tran.Complete();
                return result;
            }
        }

        public object Execute(Expression expression)
        {
            return Execute<object>(expression);
        }
    }
}