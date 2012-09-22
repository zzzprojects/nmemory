using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Modularity;
using System.Collections;
using System.Linq.Expressions;
using NMemory.Tables;
using NMemory.Execution;
using NMemory.Transactions;
using NMemory.Common;
using NMemory.Common.Visitors;

namespace NMemory.StoredProcedures
{
    public class SharedStoredProcedure<TDatabase, TResult> : ISharedStoredProcedure, ISharedStoredProcedure<TResult>
        where TDatabase : IDatabase
    {
        private Expression expression;
        private IList<Type> entityTypes;

        private IExecutionPlan<IEnumerable<TResult>> plan;
        private IList<ParameterDescription> parameters;

        public SharedStoredProcedure(Expression<Func<TDatabase, IQueryable<TResult>>> expression, bool precompile)
        {
            this.expression = expression.Body;

            // Create parameter description
            this.parameters = StoredProcedureParameterSearchVisitor
                .FindParameters(this.expression)
                .Select(p => new ParameterDescription(p.Name, p.Type))
                .ToList()
                .AsReadOnly();

            if (precompile)
            {
                this.plan = this.Compile(null);
            }
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
            IExecutionPlan<IEnumerable<TResult>> plan = this.plan;

            // If the query is not compiled, it has to be done now
            if (plan == null)
            {
                plan = this.Compile(database);
            }

            using (var tran = Transaction.EnsureTransaction(ref transaction, database))
            {
                IExecutionContext context =
                    new ExecutionContext(database, transaction, this.entityTypes, parameters);

                IEnumerable<TResult> result =
                    database.DatabaseEngine.Executor.Execute<TResult>(plan, context)
                    .ToEnumerable();

                tran.Complete();
                return result;
            }
        }

        protected IExecutionPlan<IEnumerable<TResult>> Compile(IDatabase database)
        {
            if (database == null)
            {
                database = Activator.CreateInstance<TDatabase>();
            }

            this.entityTypes = TableSearchVisitor.FindEntityTypes(this.expression);

            return database.DatabaseEngine.Compiler.Compile<IEnumerable<TResult>>(this.expression);
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
