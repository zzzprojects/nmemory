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
        private IDatabaseEngine databaseEngine;
        private StoredProcedureCollection storedProcedures;
        private TableCollection tables;
        
        /// <summary>
        /// Initializes a new instance of the <c>Database</c> class.
        /// </summary>
        public Database() : this(new DefaultDatabaseComponentFactory())
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <c>Database</c> class, using the specified database engine factory.
        /// </summary>
        /// <param name="databaseComponentFactory"></param>
        public Database(IDatabaseComponentFactory databaseComponentFactory)
        {
            if (databaseComponentFactory == null)
            {
                throw new ArgumentNullException("databaseComponentFactory");
            }

            this.databaseEngine = new DefaultDatabaseEngine(databaseComponentFactory, this);
            this.storedProcedures = new StoredProcedureCollection(this);
            this.tables = new TableCollection(this);
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

        public IDatabaseEngine DatabaseEngine
        {
            get { return this.databaseEngine; }
        }
    }

}
