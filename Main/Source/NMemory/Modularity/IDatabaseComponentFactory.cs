
namespace NMemory.Modularity
{
    /// <summary>
    /// Provides funtionality to instantiate the components of a database engine.
    /// </summary>
    public interface IDatabaseComponentFactory
    {
        /// <summary>
        /// Creates a concurrency manager that handles the concurrent access of transactions.
        /// </summary>
        /// <returns>A concurrency manager.</returns>
        IConcurrencyManager CreateConcurrencyManager();

        /// <summary>
        /// Creates a table factory module that initializes database tables.
        /// </summary>
        /// <returns>A table factory module.</returns>
        ITableFactory CreateTableFactory();

        /// <summary>
        /// Creates a query compiler that optimizes and compiles database queries.
        /// </summary>
        /// <returns>A query compiler.</returns>
        IQueryCompiler CreateQueryCompiler();

        /// <summary>
        /// Creates a query executor that executes compiled queries.
        /// </summary>
        /// <returns>A query executor.</returns>
        IQueryExecutor CreateQueryExecutor();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ILoggingPort CreateLoggingPort();


    }
}
