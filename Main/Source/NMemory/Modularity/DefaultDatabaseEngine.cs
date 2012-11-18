// ----------------------------------------------------------------------------------
// <copyright file="DefaultDatabaseEngine.cs" company="NMemory Team">
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
