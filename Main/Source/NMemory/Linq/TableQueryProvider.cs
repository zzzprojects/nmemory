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
            // TODO: Extract trasaction object

            using (var tran = Transaction.EnsureTransaction(ref transaction, this.database))
            {
                IList<ITable> tables = TableSearchVisitor.FindTables(expression);
                Func<IExecutionContext, TResult> compiledQuery = this.database.Compiler.Compile<TResult>(expression);
                IExecutionContext context = new ExecutionContext(transaction, tables);

                TResult result = compiledQuery.Invoke(context);

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