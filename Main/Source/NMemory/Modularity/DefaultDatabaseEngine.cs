using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Transactions;
using NMemory.Diagnostics;

namespace NMemory.Modularity
{
    public sealed class DefaultDatabaseEngine : IDatabaseEngine
    {
        private IQueryCompiler compiler;
        private IQueryExecutor executor;
        private IConcurrencyManager concurrencyManager;
        private ITableFactory tableFactory;
        private ITransactionHandler transactionHandler;
        private ILoggingPort loggingPort;

        public DefaultDatabaseEngine(IDatabaseComponentFactory databaseEngineFactory, IDatabase database)
        {
            this.compiler = databaseEngineFactory.CreateQueryCompiler();
            this.executor = databaseEngineFactory.CreateQueryExecutor();     
            this.concurrencyManager = databaseEngineFactory.CreateConcurrencyManager();
            this.tableFactory = databaseEngineFactory.CreateTableFactory();
            this.transactionHandler = databaseEngineFactory.CreateTransactionHandler();
            this.loggingPort = databaseEngineFactory.CreateLoggingPort();

            this.compiler.Initialize(database);
            this.executor.Initialize(database);
            this.concurrencyManager.Initialize(database);
            this.tableFactory.Initialize(database);
            this.transactionHandler.Initialize(database);
        }

        public ITableFactory TableFactory
        {
            get { return this.tableFactory; }
        }

        public IConcurrencyManager ConcurrencyManager
        {
            get { return this.concurrencyManager; }
        }

        public IQueryCompiler Compiler
        {
            get { return this.compiler; }
        }

        public IQueryExecutor Executor
        {
            get { return this.executor; }
        }

        public ITransactionHandler TransactionHandler
        {
            get { return this.transactionHandler; }
        }

        public ILoggingPort LoggingPort
        {
            get { return this.loggingPort; }
        }
    }
}
