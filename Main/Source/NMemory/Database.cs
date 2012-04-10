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
            this.tables = new TableCollection();
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

        /// <summary>
        /// Initializes a table.
        /// </summary>
        /// <typeparam name="TEntity">Specifies the type of the entities of the table.</typeparam>
        /// <typeparam name="TPrimaryKey">Specifies the type of the primary key of the entities.</typeparam>
        /// <param name="primaryKey">An expression to determine the primary key of the entities.</param>
        /// <param name="identitySpecification">An <c>IdentitySpecification</c> to set an identity field.</param>
        /// <param name="initialEntities">An <c>IEnumerable</c> to set the initial entities of the table.</param>
        /// <returns>An <c>Table</c> that represents the defined table object.</returns>
        public Table<TEntity, TPrimaryKey> CreateTable<TEntity, TPrimaryKey>( 

            Expression<Func<TEntity, TPrimaryKey>> primaryKey,
            IdentitySpecification<TEntity> identitySpecification = null,
            IEnumerable<TEntity> initialEntities = null)

            where TEntity : class
        {
            Table<TEntity, TPrimaryKey> table = this.core.CreateTable<TEntity, TPrimaryKey>(
                primaryKey, 
                identitySpecification, 
                initialEntities);

            this.tables.RegisterTable(table);

            return table;
        }

        /// <summary>
        /// Creates a relation between two tables.
        /// </summary>
        /// <typeparam name="TPrimary">The type of the entities of the primary table.</typeparam>
        /// <typeparam name="TPrimaryKey">The type of the primary key of the entities of the primary table.</typeparam>
        /// <typeparam name="TForeign">The type of the entities of the foreign table.</typeparam>
        /// <typeparam name="TForeignKey">Type type of the foreign key of the foreign table.</typeparam>
        /// <param name="primaryIndex">An <c>IIndex</c> that specifies the primary key.</param>
        /// <param name="foreignIndex">An <c>IIndex</c> that specifies the foreign key.</param>
        /// <param name="convertForeignToPrimary">A function to convert a foreign key to the corresponding primary key.</param>
        /// <param name="convertPrimaryToForeign">A function to convert a primary key to the corresponding foreign key.</param>
        /// <returns></returns>
        public Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey> CreateRelation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
                IUniqueIndex<TPrimary, TPrimaryKey> primaryIndex,
                IIndex<TForeign, TForeignKey> foreignIndex,
                Func<TForeignKey, TPrimaryKey> convertForeignToPrimary,
                Func<TPrimaryKey, TForeignKey> convertPrimaryToForeign
            )
            where TPrimary : class
            where TForeign : class
        {
            var relation = new Relation<TPrimary, TPrimaryKey, TForeign, TForeignKey>(
                primaryIndex, 
                foreignIndex, 
                convertForeignToPrimary, 
                convertPrimaryToForeign);

            this.tables.RegisterRelation(relation);

            return relation;
        }


    }

}
