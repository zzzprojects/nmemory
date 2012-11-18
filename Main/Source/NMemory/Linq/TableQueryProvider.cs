// ----------------------------------------------------------------------------------
// <copyright file="TableQueryProvider.cs" company="NMemory Team">
//     Copyright (C) 2012 NMemory Team
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
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Execution;
    using NMemory.Modularity;
    using NMemory.Transactions;

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
