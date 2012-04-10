using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NMemory.Diagnostics;
using NMemory.Indexes;
using NMemory.Modularity;
using NMemory.StoredProcedures;
using NMemory.Tables;
using NMemory.Transactions;

namespace NMemory
{
    /// <summary>
    /// Represents an NMemory database instance.
    /// </summary>
    public class Database : IDatabase
    {
        private StoredProcedureCollection storedProcedures;

        private IQueryCompiler compiler;
        private IQueryExecutor executor;
        private IConcurrencyManager concurrencyManager;
        private ICore core;

        private TransactionHandler transactionHandler;

        private TableCollection tables;
        private ILoggingPort loggingPort;


        /// <summary>
        /// Initializes a new instance of the <c>Database</c> class.
        /// </summary>
        public Database() : this(new DefaultDatabaseEngineFactory())
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <c>Database</c> class, using the specified database engine factory.
        /// </summary>
        /// <param name="databaseEngineFactory"></param>
        public Database(IDatabaseEngineFactory databaseEngineFactory)
        {
            if (databaseEngineFactory == null)
            {
                throw new ArgumentNullException("databaseEngineFactory");
            }

            this.compiler = databaseEngineFactory.CreateQueryCompiler();
            this.compiler.Initialize(this);

            this.executor = databaseEngineFactory.CreateQueryExecutor();
            this.executor.Initialize(this);

            this.concurrencyManager = databaseEngineFactory.CreateConcurrencyManager();
            this.concurrencyManager.Initialize(this);

            this.core = databaseEngineFactory.CreateCore();
            this.core.Initialize(this);

            this.transactionHandler = new TransactionHandler();
            this.transactionHandler.Initialize(this);

            this.loggingPort = new ConsoleLoggingPort();

            this.storedProcedures = new StoredProcedureCollection(this);
            this.tables = new TableCollection(this);
        }


        ICore IDatabase.Core
        {
            get { return this.core; }
        }

        IConcurrencyManager IDatabase.ConcurrencyManager
        {
            get { return this.concurrencyManager; }
        }

        IQueryCompiler IDatabase.Compiler
        {
            get { return this.compiler; }
        }

        IQueryExecutor IDatabase.Executor
        {
            get { return this.executor; }
        }

        TransactionHandler IDatabase.TransactionHandler
        {
            get { return this.transactionHandler; }
        }

        ILoggingPort IDatabase.LoggingPort 
        {
            get { return this.loggingPort; }
        }

        /// <summary>
        /// Gets a <c>StoredProcedureCollection</c> that contains the stored procedures of the database.
        /// </summary>
        public StoredProcedureCollection StoredProcedures
        {
            get { return this.storedProcedures; }
        }

        /// <summary>
        /// Gets a <c>TableCollection</c> that contains the tables of the database.
        /// </summary>
        public TableCollection Tables
        {
            get { return this.tables; }
        }
    }

}
