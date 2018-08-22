// -----------------------------------------------------------------------------------
// <copyright file="ITable`1.cs" company="NMemory Team">
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
    using System.Linq;
    using NMemory.Indexes;
    using NMemory.Transactions;

    /// <summary>
    ///     Defines an interface for database tables.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of the entities contained by the table.
    /// </typeparam>
    public interface ITable<TEntity> : ITable, IOrderedQueryable<TEntity>
        where TEntity : class
    {
        /// <summary>
        ///     Gets the primary key index of the table.
        /// </summary>
        new IUniqueIndex<TEntity> PrimaryKeyIndex { get; }

        /// <summary>
        ///     Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">
        ///     An entity that contains the property values of the new entity.
        /// </param>
        void Insert(TEntity entity);

        /// <summary>
        ///     Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">
        ///     An entity that contains the property values of the new entity.
        /// </param>
        /// <param name="entity">
        ///     The transaction within which the insert operation executes.
        /// </param>
        void Insert(TEntity entity, Transaction transaction);

        /// <summary>
        ///     Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="entity">
        ///     An entity that contains the primary key of the entity to be updated and the new
        ///     property values.
        /// </param>
        void Update(TEntity entity);

        /// <summary>
        ///     Updates the properties of the specified contained by the table.
        /// </summary>
        /// <param name="entity">
        ///     An entity that contains the primary key of the entity to be updated and the new
        ///     property values.
        /// </param>
        /// <param name="entity">
        ///     The transaction within which the update operation executes.
        /// </param>
        void Update(TEntity entity, Transaction transaction);

        /// <summary>
        ///     Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">
        ///     An entity that contains the primary key of the entity to be deleted.
        /// </param>
        void Delete(TEntity entity);

        /// <summary>
        ///     Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">
        ///     An entity that contains the primary key of the entity to be deleted.
        /// </param>
        /// <param name="entity">
        ///     The transaction within which the delete operation executes.
        /// </param>
        void Delete(TEntity entity, Transaction transaction);


        /// <summary>
        ///     Gets the collection of contraints registered to table.
        /// </summary>
        /// <value>
        ///     The contraints.
        /// </value>
        ConstraintCollection<TEntity> Contraints { get; }
    }
}
