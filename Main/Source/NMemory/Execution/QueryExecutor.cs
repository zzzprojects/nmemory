// ----------------------------------------------------------------------------------
// <copyright file="QueryExecutor.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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

namespace NMemory.Execution
{
    using System;
    using System.Collections.Generic;
    using NMemory.Common;
    using NMemory.Modularity;
    using NMemory.Tables;
    using NMemory.Transactions;

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
                cloner = EntityPropertyCloner<T>.Instance;
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
