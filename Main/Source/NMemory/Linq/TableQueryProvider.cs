using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common.Visitors;
using NMemory.Execution;
using NMemory.Tables;
using NMemory.Transactions;

namespace NMemory.Linq
{
	public class TableQueryProvider : IQueryProvider
	{
        private Database database;

		public TableQueryProvider(Database database)
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
            using (var tran = Transaction.EnsureTransaction(this.database))
            {
                IList<ITable> tables = TableSearchVisitor.FindTables(expression);
                Func<IExecutionContext, TResult> compiledQuery = this.database.Compiler.Compile<TResult>(expression);
                TResult result = compiledQuery.Invoke(new ExecutionContext(tables));

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