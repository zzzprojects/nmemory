// ----------------------------------------------------------------------------------
// <copyright file="Database.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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

using NMemory.Indexes;

namespace NMemory
{
    using System;
    using NMemory.Modularity;
    using NMemory.StoredProcedures;
    using NMemory.Tables;

    /// <summary>
    /// Represents an NMemory database instance.
    /// </summary>
    public class Database : IDatabase
    {
        private IDatabaseEngine databaseEngine;
        private StoredProcedureCollection storedProcedures;
        private TableCollection tables;

	    static Database() { //need to register this Delegate to separate Implementation from Interface
		    ModularKeyInfoFactory.Register();
	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database" /> class.
        /// </summary>
        public Database() : this(new DefaultDatabaseComponentFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database" /> class with the specified database engine factory..
        /// </summary>
        /// <param name="databaseComponentFactory">The database component factory.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="databaseComponentFactory"/> is null.</exception>
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
        /// Gets the collection of stored procedures contained by the database
        /// </summary>
        public StoredProcedureCollection StoredProcedures
        {
            get { return this.storedProcedures; }
        }

        /// <summary>
        /// Gets the collection of tables contained by the database.
        /// </summary>
        /// <value>
        /// A collection of tables.
        /// </value>
        public TableCollection Tables
        {
            get { return this.tables; }
        }

        /// <summary>
        /// Gets the database engine.
        /// </summary>
        /// <value>
        /// The database engine.
        /// </value>
        public IDatabaseEngine DatabaseEngine
        {
            get { return this.databaseEngine; }
        }
    }
}
