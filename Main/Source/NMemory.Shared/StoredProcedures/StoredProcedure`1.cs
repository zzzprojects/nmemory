// ----------------------------------------------------------------------------------
// <copyright file="StoredProcedure`1.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Common;
    using NMemory.Common.Visitors;
    using NMemory.Execution;
    using NMemory.Linq;
    using NMemory.Modularity;
    using NMemory.Transactions;
    using NMemory.Exceptions;

    /// <summary>
    ///     Represents a stored procedure that returns with a result set.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the result set elements.
    /// </typeparam>
    public class StoredProcedure<T> : 
        StoredProcedureBase,
        IStoredProcedure<T>, 
        IStoredProcedure
    {
        private TableQuery<T> query;

        public StoredProcedure(IQueryable<T> query, bool precompile)
        {
            IDatabase database = ((ITableQuery)query).Database;
            Expression expression = query.Expression;

            // Set parameter description
            this.SetParameters(
                StoredProcedureParameterSearchVisitor
                    .FindParameters(expression)
                    .Select(p => new ParameterDescription(p.Name, p.Type)));

            this.query = new TableQuery<T>(database, expression, precompile);

            if (precompile)
            {
                this.query.Compile();
            }
        }

        public IEnumerable<T> Execute(
            IDictionary<string, object> parameters)
        {
            return this.Execute(parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public IEnumerable<T> Execute(
            IDictionary<string, object> parameters, 
            Transaction transaction)
        {
            // Verify parameters
            this.VerifyParameters(parameters);

            return this.query.Execute(parameters, transaction);
        }

        IEnumerable IStoredProcedure.Execute(
            IDictionary<string, object> parameters)
        {
            return this.Execute(parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        IEnumerable IStoredProcedure.Execute(
            IDictionary<string, object> parameters, 
            Transaction transaction)
        {
            return this.Execute(parameters, transaction);
        }

        
    }
}
