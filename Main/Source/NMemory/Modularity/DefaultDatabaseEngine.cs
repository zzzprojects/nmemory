// ----------------------------------------------------------------------------------
// <copyright file="DefaultDatabaseEngine.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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
    using System.Collections;
    using System.Linq;

    public sealed class DefaultDatabaseEngine : IDatabaseEngine
    {
        private IQueryCompiler compiler;
        private IQueryExecutor executor;
        private IConcurrencyManager concurrencyManager;
        private ITransactionHandler transactionHandler;
        private ILoggingPort loggingPort;
        private IServiceProvider serviceProvider;

        public DefaultDatabaseEngine(
            IDatabaseComponentFactory databaseEngineFactory, 
            IDatabase database)
        {
            this.compiler = databaseEngineFactory.CreateQueryCompiler();
            this.executor = databaseEngineFactory.CreateQueryExecutor();     
            this.concurrencyManager = databaseEngineFactory.CreateConcurrencyManager();
            this.transactionHandler = databaseEngineFactory.CreateTransactionHandler();
            this.loggingPort = databaseEngineFactory.CreateLoggingPort();
            this.serviceProvider = databaseEngineFactory.CreateServiceProvider();

            foreach (IDatabaseComponent component in 
                this.Components.OfType<IDatabaseComponent>())
            {
                component.Initialize(database);
            }
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

        public IServiceProvider ServiceProvider
        {
            get { return this.serviceProvider; }
        }

        public IEnumerable Components
        {
            get 
            {
                if (this.concurrencyManager != null)
                {
                    yield return this.concurrencyManager;
                }

                if (this.compiler != null)
                {
                    yield return this.compiler;
                }

                if (this.executor != null)
                {
                    yield return this.executor;
                }

                if (this.transactionHandler != null)
                {
                    yield return this.transactionHandler;
                }

                if (this.loggingPort != null)
                {
                    yield return this.loggingPort;
                }

                if (this.serviceProvider != null)
                {
                    yield return this.serviceProvider;
                }
            }
        }
    }
}
