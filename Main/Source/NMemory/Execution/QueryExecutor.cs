using System;
using System.Collections.Generic;
using NMemory.Modularity;
using NMemory.Tables;
using NMemory.Transactions;
using NMemory.Linq;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common.Visitors;
using NMemory.Common;

namespace NMemory.Execution
{
    public class QueryExecutor : IQueryExecutor
    {
        private IDatabase database;

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }

        public IEnumerator<T> Execute<T>(IExecutionPlan<IEnumerable<T>> plan, IExecutionContext context)
        {
            Transaction transaction = context.Transaction;

            if (transaction == null)
            {
                throw new InvalidOperationException();
            }

            IEnumerable<T> query = plan.Execute(context);
            IConcurrencyManager cm = this.database.DatabaseEngine.ConcurrencyManager;

            ITable[] tables = TableLocator.FindAffectedTables(context.Database, plan);

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


        public T Execute<T>(IExecutionPlan<T> plan, IExecutionContext context)
        {
            Transaction transaction = context.Transaction;

            if (transaction == null)
            {
                throw new InvalidOperationException();
            }

            // TODO: Proper implementation

            return plan.Execute(context);
        }
    }
}
