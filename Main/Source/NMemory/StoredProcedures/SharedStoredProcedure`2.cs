using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common;
using NMemory.Common.Visitors;
using NMemory.Execution;
using NMemory.Modularity;
using NMemory.Transactions;
using NMemory.Linq;

namespace NMemory.StoredProcedures
{
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
            return Execute(database, parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public IEnumerable<TResult> Execute(IDatabase database, IDictionary<string, object> parameters, Transaction transaction)
        {
            // A shared stored procedure creates the query everytime the Execute method was called
            TableQuery<TResult> query = new TableQuery<TResult>(database, this.expression);

            return query.Execute(parameters, transaction);
        }

        IEnumerable ISharedStoredProcedure.Execute(IDatabase database, IDictionary<string, object> parameters)
        {
            return Execute(database, parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        IEnumerable ISharedStoredProcedure.Execute(IDatabase database, IDictionary<string, object> parameters, Transaction transaction)
        {
            return Execute(database, parameters);
        }
    }
}
