// ----------------------------------------------------------------------------------
// <copyright file="IDatabaseComponentFactory.cs" company="NMemory Team">
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

namespace NMemory.Modularity
{
    using System.Collections;

    /// <summary>
    /// Provides functionality to instantiate the components of a database engine.
    /// </summary>
    public interface IDatabaseComponentFactory
    {
        /// <summary>
        /// Creates a concurrency manager that handles the concurrent access of transactions.
        /// </summary>
        /// <returns>A concurrency manager.</returns>
        IConcurrencyManager CreateConcurrencyManager();

        /// <summary>
        /// Creates a query compiler that optimizes and compiles database queries.
        /// </summary>
        /// <returns>A query compiler.</returns>
        IQueryCompiler CreateQueryCompiler();

        /// <summary>
        /// Creates a query executor that executes compiled queries.
        /// </summary>
        /// <returns>A query executor.</returns>
        ICommandExecutor CreateQueryExecutor();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ITransactionHandler CreateTransactionHandler();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ILoggingPort CreateLoggingPort();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IServiceProvider CreateServiceProvider();
    }
}
