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
        private IDatabase database;

        private Expression expression;

        private IExecutionPlan<IEnumerable<T>> plan;
        private IList<ParameterDescription> parameters;

        public StoredProcedure(IQueryable<T> query, bool precompile)
        {
            this.database = ((ITableQuery)query).Database;
            this.expression = query.Expression;

            // Create parameter description
            this.parameters = StoredProcedureParameterSearchVisitor
                .FindParameters(this.expression)
                .Select(p => new ParameterDescription(p.Name, p.Type))
                .ToList()
                .AsReadOnly();

            if (precompile)
            {
                this.plan = this.Compile();
            }
        }

        public IEnumerable<T> Execute(IDictionary<string, object> parameters)
        {
            return Execute(parameters, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public IEnumerable<T> Execute(IDictionary<string, object> parameters, Transaction transaction)
        {
            IExecutionPlan<IEnumerable<T>> localPlan = this.plan;

            // If the query is not compiled, it has to be done now
            if (localPlan == null)
            {
                localPlan = this.Compile();
            }

            using (var tran = Transaction.EnsureTransaction(ref transaction, this.database))
            {
                IExecutionContext context = 
                    new ExecutionContext(this.database, transaction, parameters);
                
                IEnumerable<T> result = 
                    this.database.DatabaseEngine.Executor.Execute<T>(localPlan, context)
                    .ToEnumerable();

                tran.Complete();
                return result;
            }
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

        protected IExecutionPlan<IEnumerable<T>> Compile()
        {
            return this.database.DatabaseEngine.Compiler.Compile<IEnumerable<T>>(this.expression);
        }
    }
}
