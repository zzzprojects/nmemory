// ----------------------------------------------------------------------------------
// <copyright file="DefaultTable.cs" company="NMemory Team">
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

namespace NMemory.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Exceptions;
    using NMemory.Execution;
    using NMemory.Indexes;
    using NMemory.Modularity;
    using NMemory.Services.Contracts;
    using NMemory.Transactions;
    using NMemory.Transactions.Logs;

    /// <summary>
    ///     Represents a database table.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of the entities contained by the table
    /// </typeparam>
    /// <typeparam name="TPrimaryKey">
    ///     The type of the primary key of the entities.
    /// </typeparam>
    public class DefaultTable<TEntity, TPrimaryKey> : 
        Table<TEntity, TPrimaryKey> 
        where TEntity : class
    {
        public DefaultTable(
            IDatabase database,
            IKeyInfo<TEntity, TPrimaryKey> primaryKey,
            IdentitySpecification<TEntity> identitySpecification,
            object tableInfo =null)

            : base(database, primaryKey, identitySpecification, tableInfo)
        {
        }

        /// <summary>
        ///     Prevents a default instance of the <see cref="DefaultTable{TPrimaryKey}" /> 
        ///     class from being created.
        /// </summary>
        private DefaultTable()
            : base(null, null, null)
        {

        }

        /// <summary>
        ///     Core implementation of an entity insert.
        /// </summary>
        /// <param name="entity">
        ///     The entity that contains the primary key of the entity to be deleted.
        /// </param>
        /// <param name="transaction">
        ///     The transaction within which the update operation executes.
        /// </param>
        protected override void InsertCore(TEntity entity, Transaction transaction)
        {
            IExecutionContext context = 
                new ExecutionContext(this.Database, transaction, OperationType.Insert);

            if(NMemoryManager.DisableObjectCloning)
            {
                this.Executor.ExecuteInsert(entity, context);
            }
            else
            { 
                TEntity storedEntity = this.CreateStoredEntity();

                Action<TEntity, TEntity> cloner = this.EntityService.CloneProperties<TEntity>;

                cloner(entity, storedEntity);
                this.Executor.ExecuteInsert(storedEntity, context);
                cloner(storedEntity, entity);
            }
        }

        /// <summary>
        ///     Core implementation of a bulk entity update.
        /// </summary>
        /// <param name="expression">
        ///     A query expression that represents the entities to be updated.
        /// </param>
        /// <param name="updater">
        ///     An expression that represents the update mechanism.
        /// </param>
        /// <param name="transaction">
        ///     The transaction within which the update operation is executed.
        /// </param>
        /// <returns>
        ///     The updated entities.
        /// </returns>
        protected override IEnumerable<TEntity> UpdateCore(
            Expression expression, 
            IUpdater<TEntity> updater, 
            Transaction transaction)
        {
            IExecutionContext context =
                new ExecutionContext(this.Database, transaction, OperationType.Update);

            var query = this.Compiler.Compile<IEnumerable<TEntity>>(expression);

            return this.Executor.ExecuteUpdater(query, updater, context);
        }

        /// <summary>
        ///     Core implementation of an entity delete.
        /// </summary>
        /// <param name="key">
        ///     The primary key of the entity to be deleted.
        /// </param>
        /// <param name="transaction">
        ///     The transaction within which the delete operation is executed.
        /// </param>
        protected override int DeleteCore(
            Expression expression, 
            Transaction transaction)
        {
            IExecutionContext context = 
                new ExecutionContext(
                    this.Database, 
                    transaction,
                    OperationType.Delete);

            var query = this.Compiler.Compile<IEnumerable<TEntity>>(expression);

            return this.Executor.ExecuteDelete(query, context).Count();
        }

        protected ICommandExecutor Executor
        {
            get { return this.Database.DatabaseEngine.Executor; }
        }

        protected IQueryCompiler Compiler
        {
            get { return this.Database.DatabaseEngine.Compiler; }
        }
    }
}
