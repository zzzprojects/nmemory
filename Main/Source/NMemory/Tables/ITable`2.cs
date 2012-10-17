// -----------------------------------------------------------------------------------
// <copyright file="ITable`2.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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
    using NMemory.Indexes;
    using NMemory.Transactions;

    /// <summary>
    /// Defines an interface for database tables.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entities contained by the table.</typeparam>
    /// <typeparam name="TPrimaryKey">The type of the primary key of the entities.</typeparam>
    public interface ITable<TEntity, TPrimaryKey> : ITable<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Gets the primary key index of the table.
        /// </summary>
        new IUniqueIndex<TEntity, TPrimaryKey> PrimaryKeyIndex { get; }

        /// <summary>
        /// Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="key">The primary key of the entity to be updated.</param>
        /// <param name="entity">An entity that contains the new property values.</param>
        void Update(TPrimaryKey key, TEntity entity);

        /// <summary>
        /// Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="key">The primary key of the entity to be updated.</param>
        /// <param name="entity">An entity that contains the new property values.</param>
        /// <param name="entity">The transaction within which the update operation is executed.</param>
        void Update(TPrimaryKey key, TEntity entity, Transaction transaction);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="key">The primary key of the entity to be deleted.</param>
        void Delete(TPrimaryKey key);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="key">The primary key of the entity to be deleted.</param>
        /// <param name="entity">The transaction within which the update operation is executed.</param>
        void Delete(TPrimaryKey key, Transaction transaction);

    }
}
