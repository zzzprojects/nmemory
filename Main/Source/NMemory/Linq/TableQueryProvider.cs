using System;
using System.Linq;
using System.Linq.Expressions;

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
            return (TResult)this.database.Executor.Compile(expression);
        }

        public object Execute(Expression expression)
        {
            return this.Execute<object>(expression);
        }
    }
}