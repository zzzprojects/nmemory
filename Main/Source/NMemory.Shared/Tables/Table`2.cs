// -----------------------------------------------------------------------------------
// <copyright file="Table`2.cs" company="NMemory Team">
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
    using System.Diagnostics;
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
    using NMemory.Services.Contracts;
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
        private readonly IdentitySpecification<TEntity> originalIdentitySpecification;
        private readonly IUniqueIndex<TEntity, TPrimaryKey> primaryKeyIndex;
        private readonly IList<IIndex<TEntity>> indexes;
        private readonly IEntityService entityService;
        private readonly ConstraintCollection<TEntity> constraints;

        private static int counter;
        private int id;
            
        public object TableInfo { get; set; }
            
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
            IdentitySpecification<TEntity> identitySpecification,
            object tableInfo = null)

            : base(database, false)
        {
            this.TableInfo = tableInfo;
            this.VerifyType();

            this.id = Interlocked.Increment(ref counter);

            this.indexes = new List<IIndex<TEntity>>();
            this.constraints = new ConstraintCollection<TEntity>();
            this.entityService = database
                .DatabaseEngine
                .ServiceProvider
                .GetService<IEntityService>();

            this.primaryKeyIndex = 
                this.CreateUniqueIndex(new DictionaryIndexFactory(), primaryKey);

            this.RegisterTimestampConstraints();

            if (identitySpecification != null)
            {
                this.identityField = new IdentityField<TEntity>(identitySpecification);
                this.originalIdentitySpecification = identitySpecification;
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

        internal IEnumerable<TEntity> Execute(IQueryable<TEntity> query, Transaction transaction)
        {
            IEnumerable<TEntity> entities = QueryableEx.Execute(query, transaction);

            return entities;
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
            var updater = new EntityUpdater<TEntity>(entity);
            var query = this.CreateQuery(key);

            var updated = this.Update(query, updater, transaction).SingleOrDefault();

            if (updated != null)
            {
                this.EntityService.CloneProperties(updated, entity);
            }
        }

        /// <summary>
        ///     Updates the entities.
        /// </summary>
        /// <param name="query">
        ///     The query expression that represents the entities to be updated.
        /// </param>
        /// <param name="updater">
        ///     The expression that represents the update logic.
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

            var updaterObj = new ExpressionUpdater<TEntity>(updater);

            return this.Update((IQueryable<TEntity>)query, updaterObj, transaction);
        }

        private IEnumerable<TEntity> Update(IQueryable<TEntity> query, IUpdater<TEntity> updater, Transaction transaction)
        {
            try
            {
                using (var tran = Transaction.EnsureTransaction(ref transaction, this.Database))
                {
                    IEnumerable<TEntity> result = this.UpdateCore(query.Expression, updater, transaction);

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
        ///     Core implementation of a bulk entity update.
        /// </summary>
        /// <param name="expression">
        ///     The query expression that represents the entities to be updated.
        /// </param>
        /// <param name="updater">
        ///     The updater.
        /// </param>
        /// <param name="transaction">
        ///     The transaction within which the update operation is executed.
        /// </param>
        /// <returns> The updated entities. </returns>
        protected abstract IEnumerable<TEntity> UpdateCore(Expression expression, IUpdater<TEntity> updater, Transaction transaction);

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
            var query = this.CreateQuery(key);

            this.Delete(query, transaction);
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
            return this.Delete(((IQueryable<TEntity>)query), transaction);
        }

        private int Delete(IQueryable<TEntity> query, Transaction transaction)
        {
            try
            {
                using (var tran = Transaction.EnsureTransaction(ref transaction, this.Database))
                {
                    int result = this.DeleteCore(query.Expression, transaction);

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
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
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
        /// <param name="keySelector">
        ///     The expression representing the definition of the index key.
        /// </param>
        /// <returns>
        ///     The index.
        /// </returns>
        public IIndex<TEntity, TKey> CreateIndex<TKey>(
            IIndexFactory indexFactory,
            Expression<Func<TEntity, TKey>> keySelector)
        {
            var keyFactory = new ModularKeyInfoFactory(this.Database);
            var key = keyFactory.Create(keySelector);
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
            Expression<Func<TEntity, TUniqueKey>> keySelector)
        {
            var keyFactory = new ModularKeyInfoFactory(this.Database);
            var key = keyFactory.Create(keySelector);
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

        public ConstraintCollection<TEntity> Contraints
        {
            get { return this.constraints; }
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

        protected void CalculateIdentityFeed(bool forceMinValue = false)
        {
            if (this.identityField != null)
            {
                this.identityField.InitializeBasedOnData(this.primaryKeyIndex.SelectAll(), forceMinValue);                                
            }
        }

        protected IQueryable<TEntity> CreateQuery(TPrimaryKey key)
        {
            var param = Expression.Parameter(typeof(TEntity));
            var members = this.PrimaryKeyIndex.KeyInfo.EntityKeyMembers;

            var selector =
                KeyExpressionHelper.CreateKeySelector(param, members, this.KeyInfoHelper);

            var predicate =
                Expression.Lambda<Func<TEntity, bool>>(
                    Expression.Equal(
                        selector,
                        Expression.Constant(key)),
                    param);

            return ((IQueryable<TEntity>)this).Where(predicate);
        }

        protected IKeyInfoHelper KeyInfoHelper
        {
            get
            {
                var keyInfoService = this.Database
                    .DatabaseEngine
                    .ServiceProvider
                    .GetService<IKeyInfoService>();

                IKeyInfoHelper helper;

                keyInfoService.TryCreateKeyInfoHelper(typeof(TPrimaryKey), out helper);

                if (helper == null)
                {
                    throw new NMemoryException();
                }

                return helper;
            }
        }

        protected IEntityService EntityService
        {
            get
            {
                return this.entityService;
            }
        }

        protected virtual bool SupportsPropertyType(PropertyInfo prop)
        {
            Type type = prop.PropertyType;

            if (type.IsValueType)
            {
                return true;
            }

            if (type == typeof(string))
            {
                return true;
            }

            if (type == typeof(NMemory.Data.Binary))
            {
                return true;
            }

            if (type == typeof(NMemory.Data.Timestamp))
            {
                return true;
            }

            if (type == typeof(byte[]))
            {
                throw new ArgumentException(
                    string.Format("The type of '{0}' property is '{1}', use '{2}' instead.",
                        prop.Name,
                        typeof(byte[]).FullName,
                        typeof(Binary).FullName),
                    "TEntity");
            }

            return false;
        }

        private void VerifyType()
        {
            foreach (PropertyInfo item in typeof(TEntity).GetProperties())
            {
                if (this.SupportsPropertyType(item))
                {
                    continue;
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

                this.Contraints.Add(new TimestampConstraint<TEntity>(lambda));
            }
        }
            
        public void RestoreIdentityField()
        {
            if (originalIdentitySpecification != null)
            {
                identityField = new IdentityField<TEntity>(originalIdentitySpecification);
            }
        }
    }
}
