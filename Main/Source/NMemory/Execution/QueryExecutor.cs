using System;
using System.Collections.Generic;
using NMemory.Modularity;
using NMemory.Tables;
using NMemory.Transactions;
using NMemory.Linq;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common.Visitors;

namespace NMemory.Execution
{
    public class QueryExecutor : IQueryExecutor
    {
        private IDatabase database;

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }

        public IEnumerator<T> Execute<T>(Func<IExecutionContext, IEnumerable<T>> compiledQuery, IExecutionContext context)
        {
            Transaction transaction = Transaction.Current;

            if (transaction == null)
            {
                throw new InvalidOperationException();
            }

            IEnumerable<T> query = compiledQuery.Invoke(context);
            IConcurrencyManager cm = this.database.ConcurrencyManager;

            ITable[] tables = context.Tables.ToArray();

            EntityPropertyCloner<T> cloner = null;
            if (this.database.Tables.IsEntityType<T>())
            {
                cloner = new EntityPropertyCloner<T>();
            }

            LinkedList<T> result = new LinkedList<T>();
            

            for (int i = 0; i < tables.Length; i++)
            {
                cm.AcquireTableReadLock(tables[i], transaction);
            }

            try
            {
                foreach (T item in query)
                {
                    if (cloner != null)
                    {
                        T resultEntity = Activator.CreateInstance<T>();
                        cloner.Clone(item, resultEntity);

                        result.AddLast(resultEntity);
                    }
                    else
                    {
                        result.AddLast(item);
                    }
                }
            }
            finally
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    cm.ReleaseTableReadLock(tables[i], transaction);
                }
            }

               

            return result.GetEnumerator();
        }




        public T Execute<T>(Func<IExecutionContext, T> compiledQuery, IExecutionContext context)
        {
            Transaction transaction = Transaction.Current;

            if (transaction == null)
            {
                throw new InvalidOperationException();
            }

            // TODO: Proper implementation

            return compiledQuery.Invoke(context);
        }
    }
}
