using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Modularity
{
    /// <summary>
    /// Provides funtionality to instantiate <c>Database</c> instances.
    /// </summary>
    public interface IDatabaseEngineFactory
    {
        /// <summary>
        /// Creates a concurrency manager that handles the concurrent access of transactions.
        /// </summary>
        /// <returns>A concurrency manager.</returns>
        IConcurrencyManager CreateConcurrencyManager();

        /// <summary>
        /// Creates a core module that provides basic functionalites to the database engine. 
        /// </summary>
        /// <returns>A core module.</returns>
        ICore CreateDispatcher();

        /// <summary>
        /// Creates a query executor that optimizes and executes database queries.
        /// </summary>
        /// <returns>A query executor.</returns>
        IQueryExecutor CreateQueryExecutor();
    }
}
