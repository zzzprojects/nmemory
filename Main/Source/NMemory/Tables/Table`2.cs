// -----------------------------------------------------------------------------------
// <copyright file="Table`2.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using NMemory.Common;
    using NMemory.Constraints;
    using NMemory.Data;
    using NMemory.Exceptions;
    using NMemory.Execution;
    using NMemory.Indexes;
    using NMemory.Linq;
    using NMemory.Modularity;
    using NMemory.Transactions;

    /// <summary>
    ///     Represents a database table.
    /// </summary>
    /// <typeparam name="TEntity">
    ///     The type of the entities contained by the table
    /// </typeparam>
    /// <typeparam name="TPrimaryKey">
    ///     The type of the primary key of the entities.
    /// </typeparam>
    public abstract class Table<TEntity, TPrimaryKey> :
        TableQuery<TEntity>,
        ITable<TEntity, TPrimaryKey>,
        IQueryable<TEntity>,
        IBulkTable<TEntity>,
        IReflectionTable

        where TEntity : class
    {

        #region Fields

        private IdentityField<TEntity> identityField;
        private IUniqueIndex<TEntity, TPrimaryKey> primaryKeyIndex;
        private IList<IIndex<TEntity>> indexes;
        private IList<IConstraint<TEntity>> constraints;

        private static int counter;
        private int id;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Table{TEntity, TPrimaryKey}"/> 
        ///     class.
        /// </summary>
        /// <param name="database"> The database. </param>
        /// <param name="primaryKey"> The primary key. </param>
        /// <param name="identitySpecification"> The identity specification. </param>
        public Table(
            IDatabase database,
            IKeyInfo<TEntity, TPrimaryKey> primaryKey,
            IdentitySpecification<TEntity> identitySpecification)

            : base(database, false)
        {
            this.VerifyType();

            this.id = Interlocked.Increment(ref counter);

            this.indexes = new List<IIndex<TEntity>>();
            this.constraints = new List<IConstraint<TEntity>>();

            this.primaryKeyIndex = 
                this.CreateUniqueIndex(new DictionaryIndexFactory(), primaryKey);

            this.RegisterTimestampConstraints();

            if (identitySpecification != null)
            {
                this.identityField = new IdentityField<TEntity>(identitySpecification);
            }
        }

        /// <summary>
        ///     Prevents a default instance of the <see cref="Table{TPrimaryKey}" /> class from
        ///     being created.
        /// </summary>
        private Table() : base(null)
        {

        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the indexes of the table are changed.
        /// </summary>
        public event EventHandler IndexChanged;

        #endregion

        #region Lock management

        /// <summary>
        /// Acquires the table write lock.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        protected void AcquireWriteLock(Transaction transaction)
        {
            this.Database.DatabaseEngine.ConcurrencyManager.AcquireTableWriteLock(this, transaction);
        }

        /// <summary>
        /// Releases the table write lock.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        protected void ReleaseWriteLock(Transaction transaction)
        {
            this.Database.DatabaseEngine.ConcurrencyManager.ReleaseTableWriteLock(this, transaction);
        }

        /// <summary>
        /// Acquires the table read lock.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        protected void AcquireReadLock(Transaction transaction)
        {
            this.Database.DatabaseEngine.ConcurrencyManager.AcquireTableReadLock(this, transaction);
        }

        /// <summary>
        /// Releases the table read lock.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        protected void ReleaseReadLock(Transaction transaction)
        {
            this.Database.DatabaseEngine.ConcurrencyManager.ReleaseTableReadLock(this, transaction);
        }

        #endregion

        #region Manipulation

        /// <summary>
        ///     Inserts the specified entity.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        public void Insert(TEntity entity)
        {
            this.Insert(entity, Transaction.TryGetAmbientEnlistedTransaction());
        }

        /// <summary>
        ///     Inserts the specified entity.
        /// </summary>
        /// <param name="entity"> The entity. </param>
        /// <param name="transaction"> The transaction. </param>
        public void Insert(TEntity entity, Transaction transaction)
        {
            if (this.identityField != null)
            {
                this.GenerateIdentityFieldValue(entity);
            }

            try
            {
                using (var tran = Transaction.EnsureTransaction(ref transaction, this.Database))
                {
                    this.InsertCore(entity, transaction);

                    tran.Complete();
                }
            }
            catch (System.Transactions.TransactionAbortedException)
            {
                throw;
            }
            catch (NMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NMemoryException(ErrorCode.GenericError, ex);
            }

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
        protected abstract void InsertCore(TEntity entity, Transaction transaction);

        /// <summary>
        ///     Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="entity">
        ///     An entity that contains the primary key of the entity to be updated and the new
        ///     property values.
        /// </param>
        public void Update(TEntity entity)
        {
            this.Update(entity, Transaction.TryGetAmbientEnlistedTransaction());
        }
        
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
        public void Update(TEntity entity, Transaction transaction)
        {
            TPrimaryKey key = this.primaryKeyIndex.KeyInfo.SelectKey(entity);

            this.Update(key, entity, transaction);
        }

        /// <summary>
        ///     Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="key">
        ///     The primary key of the entity to be updated.
        /// </param>
        /// <param name="entity">
        ///     An entity that contains the new property values.
        /// </param>
        public void Update(TPrimaryKey key, TEntity entity)
        {
            this.Update(key, entity, Transaction.TryGetAmbientEnlistedTransaction());
        }

        /// <summary>
        ///     Updates the properties of the specified entity contained by the table.
        /// </summary>
        /// <param name="key">
        ///     The primary key of the entity to be updated.
        /// </param>
        /// <param name="entity">
        ///     An entity that contains the new property values.
        /// </param>
        /// <param name="entity">
        ///     The transaction within which the update operation is executed.
        /// </param>
        public void Update(TPrimaryKey key, TEntity entity, Transaction transaction)
        {
            try
            {
                using (var tran = Transaction.EnsureTransaction(ref transaction, this.Database))
                {
                    this.UpdateCore(key, entity, transaction);

                    tran.Complete();
                }
            }
            catch (System.Transactions.TransactionAbortedException)
            {
                throw;
            }
            catch (NMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NMemoryException(ErrorCode.GenericError, ex);
            }
        }

        /// <summary>
        ///     Updates the entities.
        /// </summary>
        /// <param name="query">
        ///     A query expression that represents the entities to be updated.
        /// </param>
        /// <param name="updater">
        ///     An expression that represents the update logic.
        /// </param>
        /// <param name="transaction">
        ///     The transaction within which the update operation is executed.
        /// </param>
        /// <returns>
        ///     The updated entities.
        /// </returns>
        IEnumerable<TEntity> IBulkTable<TEntity>.Update(TableQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updater, Transaction transaction)
        {
            updater = ExpressionHelper.ValidateAndCompleteUpdaterExpression(updater);
            Expression expression = ((IQueryable<TEntity>)query).Expression;

            try
            {
                using (var tran = Transaction.EnsureTransaction(ref transaction, this.Database))
                {
                    IEnumerable<TEntity> result = this.UpdateCore(expression, updater, transaction);

                    tran.Complete();
                    return result;
                }
            }
            catch (System.Transactions.TransactionAbortedException)
            {
                throw;
            }
            catch (NMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NMemoryException(ErrorCode.GenericError, ex);
            }
        }

        /// <summary>
        ///     Core implementation of an entity update.
        /// </summary>
        /// <param name="key">
        ///     The primary key of the entity to be updated.
        /// </param>
        /// <param name="entity">
        ///     An entity that contains the new propery values.
        /// </param>
        /// <param name="transaction">
        ///     The transaction within which the update operation is executed.
        /// </param>
        protected abstract void UpdateCore(TPrimaryKey key, TEntity entity, Transaction transaction);

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
        /// <returns> The updated entities. </returns>
        protected abstract IEnumerable<TEntity> UpdateCore(Expression expression, Expression<Func<TEntity, TEntity>> updater, Transaction transaction);

        /// <summary>
        ///     Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">
        ///     An entity that contains the primary key of the entity to be deleted.
        /// </param>
        public void Delete(TEntity entity)
        {
            this.Delete(entity, Transaction.TryGetAmbientEnlistedTransaction());
        }

        /// <summary>
        ///     Deletes an entity from the table.
        /// </summary>
        /// <param name="key">
        ///     The primary key of the entity to be deleted.
        /// </param>
        /// <param name="entity">
        ///     The transaction within which the update operation is executed.
        /// </param>
        public void Delete(TEntity entity, Transaction transaction)
        {
            TPrimaryKey key = this.primaryKeyIndex.KeyInfo.SelectKey(entity);

            this.Delete(key, transaction);
        }

        /// <summary>
        ///     Deletes an entity from the table.
        /// </summary>
        /// <param name="key">
        ///     The primary key of the entity to be deleted.
        /// </param>
        public void Delete(TPrimaryKey key)
        {
            this.Delete(key, Transaction.TryGetAmbientEnlistedTransaction());
        }

        /// <summary>
        ///     Deletes an entity from the table.
        /// </summary>
        /// <param name="key">
        ///     The primary key of the entity to be deleted.
        /// </param>
        /// <param name="entity">
        ///     The transaction within which the update operation is executed.
        /// </param>
        public void Delete(TPrimaryKey key, Transaction transaction)
        {
            try
            {
                using (var tran = Transaction.EnsureTransaction(ref transaction, this.Database))
                {
                    this.DeleteCore(key, transaction);

                    tran.Complete();
                }
            }
            catch (System.Transactions.TransactionAbortedException)
            {
                throw;
            }
            catch (NMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NMemoryException(ErrorCode.GenericError, ex);
            }
        }

        /// <summary>
        ///     Deletes entities.
        /// </summary>
        /// <param name="query">
        ///     The query that represents the entities to be deleted.
        /// </param>
        /// <param name="transaction">
        ///     The transaction within which the delete operation is executed.
        /// </param>
        /// <returns>
        ///     The count of the deleted entities.
        /// </returns>
        int IBulkTable<TEntity>.Delete(TableQuery<TEntity> query, Transaction transaction)
        {
            Expression expression = ((IQueryable<TEntity>)query).Expression;

            try
            {
                using (var tran = Transaction.EnsureTransaction(ref transaction, this.Database))
                {
                    int result = this.DeleteCore(expression, transaction);

                    tran.Complete();
                    return result;
                }
            }
            catch (System.Transactions.TransactionAbortedException)
            {
                throw;
            }
            catch (NMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new NMemoryException(ErrorCode.GenericError, ex);
            }
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
        protected abstract void DeleteCore(TPrimaryKey key, Transaction transaction);

        /// <summary>
        ///     Core implementation of a bulk entity delete.
        /// </summary>
        /// <param name="expression">
        ///     A query expression that represents the entities to be deleted.
        /// </param>
        /// <param name="transaction">
        ///     The transaction within which the delete operation is executed.
        /// </param>
        /// <returns>
        ///     The count of deleted entities.
        /// </returns>
        protected abstract int DeleteCore(Expression expression, Transaction transaction);

        #endregion

        #region Query

        /// <summary>
        /// Gets the number of entities contained by the table.
        /// </summary>
        public long Count
        {
            get
            {
                return this.LongCount();
            }
        }

        internal long GetCount()
        {
            return this.primaryKeyIndex.Count;
        }

        #endregion

        #region Event helpers

        private void OnIndexChanged()
        {
            if (this.IndexChanged != null)
            {
                this.IndexChanged.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Index factory methods

        /// <summary>
        ///     Creates a new index.
        /// </summary>
        /// <typeparam name="TKey">
        ///     The type of the index key.
        /// </typeparam>
        /// <param name="indexFactory">
        ///     The index factory.
        /// </param>
        /// <param name="key">
        ///     The expression representing the definition of the index key.
        /// </param>
        /// <returns>
        ///     The index.
        /// </returns>
        public IIndex<TEntity, TKey> CreateIndex<TKey>(
            IIndexFactory indexFactory,
            Expression<Func<TEntity, TKey>> key)
        {
            var index = indexFactory.CreateIndex(this, key);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        /// <summary>
        ///     Creates a new index.
        /// </summary>
        /// <typeparam name="TKey"> The type of the index key. </typeparam>
        /// <param name="indexFactory"> The index factory. </param>
        /// <param name="keyInfo"> The definition of the index key. </param>
        /// <returns> The index. </returns>
        public IIndex<TEntity, TKey> CreateIndex<TKey>(
            IIndexFactory indexFactory,
            IKeyInfo<TEntity, TKey> keyInfo)
        {
            var index = indexFactory.CreateIndex(this, keyInfo);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        /// <summary>
        ///     Creates a new unique index.
        /// </summary>
        /// <typeparam name="TUniqueKey">
        ///     The type of the unique index key.
        /// </typeparam>
        /// <param name="indexFactory">
        ///     The index factory.
        /// </param>
        /// <param name="key">
        ///     The expression representing the definition of the index key.
        /// </param>
        /// <returns>
        ///     The unique index. 
        /// </returns>
        public IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
            IIndexFactory indexFactory,
            Expression<Func<TEntity, TUniqueKey>> key)
        {
            var index = indexFactory.CreateUniqueIndex(this, key);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        /// <summary>
        ///     Creates a new unique index.
        /// </summary>
        /// <typeparam name="TUniqueKey">
        ///     The type of the unqiue index key.
        /// </typeparam>
        /// <param name="indexFactory">
        ///     The index factory.
        /// </param>
        /// <param name="keyInfo">
        ///     The definition of the index key
        /// </param>
        /// <returns> The unique index. </returns>
        public IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
            IIndexFactory indexFactory,
            IKeyInfo<TEntity, TUniqueKey> keyInfo)
        {
            var index = indexFactory.CreateUniqueIndex(this, keyInfo);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        #endregion

        #region ITable Members

        /// <summary>
        ///     Gets the indexes of the table.
        /// </summary>
        public IEnumerable<IIndex> Indexes
        {
            get { return indexes; }
        }

        /// <summary>
        ///     Gets the primary key index of the table.
        /// </summary>
        public IUniqueIndex<TEntity, TPrimaryKey> PrimaryKeyIndex
        {
            get { return this.primaryKeyIndex; }
        }

        /// <summary>
        ///     Gets the index of the primary key.
        /// </summary>
        /// <value>
        ///     The index of the primary key.
        /// </value>
        IUniqueIndex<TEntity> ITable<TEntity>.PrimaryKeyIndex
        {
            get { return this.primaryKeyIndex; }
        }

        /// <summary>
        ///     Gets the index of the primary key.
        /// </summary>
        /// <value>
        ///     The index of the primary key.
        /// </value>
        IIndex ITable.PrimaryKeyIndex
        {
            get { return this.primaryKeyIndex; }
        }

        #endregion

        #region IReflectionTable Members

        /// <summary>
        /// Updates the properties of an entity contained by the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be updated and the new property values.</param>
        void IReflectionTable.Update(object entity)
        {
            this.Update((TEntity)entity);
        }

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">An entity that contains the property values of the new entity.</param>
        void IReflectionTable.Insert(object entity)
        {
            this.Insert((TEntity)entity);
        }

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be deleted.</param>
        void IReflectionTable.Delete(object entity)
        {
            this.Delete((TEntity)entity);
        }

        #endregion

        /// <summary>
        ///     Adds a table constraint.
        /// </summary>
        /// <param name="constraint">
        ///     The constraint. Note that you must not share this constraint instance across 
        ///     multiple tables.
        /// </param>
        public void AddConstraint(IConstraint<TEntity> constraint)
        {
            this.constraints.Add(constraint);
        }

        /// <summary>
        ///     Adds a table constraint.
        /// </summary>
        /// <param name="constraintFactory"> 
        ///     The constraint factory that instantiates a dedicated constraint instance for
        ///     this table.
        ///     </param>
        public void AddConstraint(IConstraintFactory<TEntity> constraintFactory)
        {
            IConstraint<TEntity> constraint = constraintFactory.Create();

            this.AddConstraint(constraint);
        }

        /// <summary>
        ///     Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Table<{0}>", this.EntityType.Name);
        }

        /// <summary>
        ///     Applies the contraints on the specified entity.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        protected void ApplyContraints(TEntity entity, IExecutionContext context)
        {
            foreach (IConstraint<TEntity> constraint in this.constraints)
            {
                constraint.Apply(entity, context);
            }
        }

        /// <summary>
        ///     Creates an entity that is meant to be stored in the table.
        /// </summary>
        /// <returns>
        ///     The entity.
        /// </returns>
        protected virtual TEntity CreateStoredEntity()
        {
            return Activator.CreateInstance<TEntity>();
        }

        protected virtual void GenerateIdentityFieldValue(TEntity entity)
        {
            if (this.identityField != null)
            {
                this.identityField.Generate(entity);
            }
        }

        protected void CalculateIdentityFeed()
        {
            if (this.identityField != null)
            {
                this.identityField.InitializeBasedOnData(this.primaryKeyIndex.SelectAll());                                
            }
        }

        private void VerifyType()
        {
            foreach (PropertyInfo item in typeof(TEntity).GetProperties())
            {
                if (item.PropertyType.IsValueType)
                {
                    continue;
                }

                if (item.PropertyType == typeof(string))
                {
                    continue;
                }

                if (item.PropertyType == typeof(NMemory.Data.Binary))
                {
                    continue;
                }

                if (item.PropertyType == typeof(NMemory.Data.Timestamp))
                {
                    continue;
                }

                if (item.PropertyType == typeof(byte[]))
                {
                    throw new ArgumentException(
                        string.Format("The type of '{0}' property is '{1}', use '{2}' instead.",
                            item.Name,
                            typeof(byte[]).FullName,
                            typeof(Binary).FullName),
                        "TEntity");
                }

                throw new ArgumentException(
                    string.Format("The type of '{0}' property is a reference type.", item.Name),
                    "TEntity");
            }
        }

        private void RegisterTimestampConstraints()
        {
            foreach (PropertyInfo item in typeof(TEntity).GetProperties())
            {
                if (item.PropertyType != typeof(Timestamp))
                {
                    continue;
                }

                var parameter = Expression.Parameter(typeof(TEntity));
                var lambda = Expression.Lambda<Func<TEntity, Timestamp>>(Expression.Property(parameter, item), parameter);

                this.constraints.Add(new TimestampConstraint<TEntity>(lambda));
            }
        }
    }
}
