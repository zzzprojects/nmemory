// ----------------------------------------------------------------------------------
// <copyright file="SharedStoredProcedure`2.cs" company="NMemory Team">
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

namespace NMemory.StoredProcedures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Common.Visitors;
    using NMemory.Linq;
    using NMemory.Modularity;
    using NMemory.Transactions;

    public class SharedStoredProcedure<TDatabase, TResult> : ISharedStoredProcedure, ISharedStoredProcedure<TResult>
        where TDatabase : IDatabase
    {
        private Expression expression;
        private IList<ParameterDescription> parameters;

        public SharedStoredProcedure(Expression<Func<TDatabase, IQueryable<TResult>>> expression)
        {
            this.expression = expression.Body;

            // Create parameter description
            this.parameters = StoredProcedureParameterSearchVisitor
                .FindParameters(this.expression)
                .Select(p => new ParameterDescription(p.Name, p.Type))
                .ToList()
                .AsReadOnly();
        }

        public IList<ParameterDescription> Parameters
        {
            get { return this.parameters; }
        }

        public IEnumerable<TResult> Execute(IDatabase database, IDictionary<string, object> parameters)
        {
            return this.Execute(database, parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public IEnumerable<TResult> Execute(IDatabase database, IDictionary<string, object> parameters, Transaction transaction)
        {
            // A shared stored procedure creates the query everytime the Execute method was called
            TableQuery<TResult> query = new TableQuery<TResult>(database, this.expression);

            return query.Execute(parameters, transaction);
        }

        IEnumerable ISharedStoredProcedure.Execute(IDatabase database, IDictionary<string, object> parameters)
        {
            return this.Execute(database, parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        IEnumerable ISharedStoredProcedure.Execute(IDatabase database, IDictionary<string, object> parameters, Transaction transaction)
        {
            return this.Execute(database, parameters);
        }
    }
}
