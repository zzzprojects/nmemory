using System;
using System.Collections.Generic;
using NMemory.Modularity;
using NMemory.Tables;
using NMemory.Transactions;
using NMemory.Linq;
using System.Linq;
using System.Linq.Expressions;

namespace NMemory.Concurrency
{
    public class QueryEnumeratorFactory : IQueryEnumeratorFactory
    {
        public IEnumerator<T> Create<T>(TableQuery<T> tableQuery, TransactionWrapper transactionWrapper)
        {
            Expression expression = ((IQueryable<T>)tableQuery).Expression;
            IEnumerable<T> query = tableQuery.Database.Executor.Compile(expression) as IEnumerable<T>;
            IConcurrencyManager cm = tableQuery.Database.ConcurrencyManager;

            // TODO: find tables
            List<ITable> tables = new List<ITable>();

            EntityPropertyCloner<T> cloner = null;
            if (tableQuery.Database.Tables.IsEntityType<T>())
            {
                cloner = new EntityPropertyCloner<T>();
            }

            LinkedList<T> result = new LinkedList<T>();

            using (transactionWrapper)
            {
                Transaction transaction = Transaction.Current;

                for (int i = 0; i < tables.Count; i++)
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
                    for (int i = 0; i < tables.Count; i++)
                    {
                        cm.ReleaseTableReadLock(tables[i], transaction);
                    }
                }

                transactionWrapper.Complete();
            }

            return result.GetEnumerator();
        }
    }
}
