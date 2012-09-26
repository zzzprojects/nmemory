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

namespace NMemory.StoredProcedures
{
    /// <summary>
    /// Represents a stored procedure that returns with a resultset.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the resultset.</typeparam>
    public class StoredProcedure<T> : IStoredProcedure<T>, IStoredProcedure
    {
        private TableQuery<T> query;
        private IList<ParameterDescription> parameters;

        public StoredProcedure(IQueryable<T> query, bool precompile)
        {
            IDatabase database = ((ITableQuery)query).Database;
            Expression expression = query.Expression;

            // Create parameter description
            this.parameters = StoredProcedureParameterSearchVisitor
                .FindParameters(expression)
                .Select(p => new ParameterDescription(p.Name, p.Type))
                .ToList()
                .AsReadOnly();

            this.query = new TableQuery<T>(database, expression, precompile);

            if (precompile)
            {
                this.query.Compile();
            }
        }

        public IEnumerable<T> Execute(IDictionary<string, object> parameters)
        {
            return Execute(parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public IEnumerable<T> Execute(IDictionary<string, object> parameters, Transaction transaction)
        {
            return this.query.Execute(parameters, transaction);
        }

        IList<ParameterDescription> IStoredProcedure.Parameters
        {
            get { return this.parameters; }
        }

        IEnumerable IStoredProcedure.Execute(IDictionary<string, object> parameters)
        {
            return Execute(parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        IEnumerable IStoredProcedure.Execute(IDictionary<string, object> parameters, Transaction transaction)
        {
            return Execute(parameters, transaction);
        }
    }
}
