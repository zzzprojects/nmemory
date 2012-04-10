using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Common;
using NMemory.Common.Visitors;
using NMemory.Execution;
using NMemory.Linq;
using NMemory.Tables;
using NMemory.Transactions;
using System.Collections;
using System.Collections.ObjectModel;
using NMemory.Modularity;

namespace NMemory.StoredProcedures
{
    /// <summary>
    /// Represents a stored procedure that returns with a resultset.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the resultset.</typeparam>
    public class StoredProcedure<T> : IStoredProcedure<T>, IStoredProcedure
    {
        private IDatabase database;
        private IList<ITable> tables;

        private Expression expression;
       
        private Func<IExecutionContext, IEnumerable<T>> compiledQuery;
        private IList<ParameterDescription> parameters;

        public StoredProcedure(IQueryable<T> query, bool precompiled)
        {
            this.database = ((ITableQuery)query).Database;
            this.expression = query.Expression;

            // Create parameter description
            this.parameters = StoredProcedureParameterSearchVisitor
                .FindParameters(this.expression)
                .Select(p => new ParameterDescription(p.Name, p.Type))
                .ToList()
                .AsReadOnly();

            this.tables = TableSearchVisitor.FindTables(this.expression);

            if (precompiled)
            {
                this.compiledQuery = this.Compile();
            }
        }

        public IEnumerable<T> Execute(IDictionary<string, object> parameters)
        {
            Func<IExecutionContext, IEnumerable<T>> compiledQuery = this.compiledQuery;

            // If the query is not compiled, it has to be done now
            if (compiledQuery == null)
            {
                compiledQuery = this.Compile();
            }

            IExecutionContext context = new ExecutionContext(this.tables, parameters);

            using (var tran = Transaction.EnsureTransaction(this.database))
            {
                IEnumerable<T> result = this.database.Executor.Execute<T>(compiledQuery, context).ToEnumerable();

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
            return Execute(parameters);
        }

        protected Func<IExecutionContext, IEnumerable<T>> Compile()
        {
            return this.database.Compiler.Compile<IEnumerable<T>>(this.expression);
        }
    }
}
