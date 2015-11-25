// -----------------------------------------------------------------------------------
// <copyright file="IBulkTable.cs" company="NMemory Team">
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
// -----------------------------------------------------------------------------------

namespace NMemory.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using NMemory.Linq;
    using NMemory.Transactions;

    /// <summary>
    /// Defines bulk operations for a table.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities contained by the table.</typeparam>
    public interface IBulkTable<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Updates the entities.
        /// </summary>
        /// <param name="query">A query expression that represents the entities to be updated.</param>
        /// <param name="updater">An expression that represents the update logic.</param>
        /// <param name="transaction">The transaction within which the update operation is executed.</param>
        /// <returns>The updated entities</returns>
        IEnumerable<TEntity> Update(TableQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updater, Transaction transaction);

        /// <summary>
        /// Deletes entities.
        /// </summary>
        /// <param name="query">The query that represents the entities to be deleted.</param>
        /// <param name="transaction">The transaction within which the delete operation is executed.</param>
        /// <returns>The count of the deleted entities.</returns>
        int Delete(TableQuery<TEntity> query, Transaction transaction);
    }
}
