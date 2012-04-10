using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using NMemory.Common;
using NMemory.Constraints;
using NMemory.Data;
using NMemory.DataStructures;
using NMemory.Exceptions;
using NMemory.Execution;
using NMemory.Indexes;
using NMemory.Linq;
using NMemory.Transactions;
using NMemory.Transactions.Logs;
using NMemory.Modularity;
using NMemory.Common.Visitors;

namespace NMemory.Tables
{
    public abstract class Table<TEntity, TPrimaryKey> :

        TableQuery<TEntity>,

        ITable<TEntity, TPrimaryKey>,
        IQueryable<TEntity>,
        IBatchTable<TEntity>,
        IReflectionTable

        where TEntity : class
    {

        #region Members

        private IdentityField<TEntity> identityField;
        private IUniqueIndex<TEntity, TPrimaryKey> primaryKeyIndex;
        private IList<IIndex<TEntity>> indexes;
        private IList<IConstraint<TEntity>> constraints;

        private static int counter;
        private int id;

        #endregion

        #region Constructor

        public Table(
            IDatabase database,
            Expression<Func<TEntity, TPrimaryKey>> primaryKey,

            IdentitySpecification<TEntity> identitySpecification,
            IEnumerable<TEntity> initialEntities)

            : base(database)
        {
            this.VerifyType();

            this.indexes = new List<IIndex<TEntity>>();
            this.constraints = new List<IConstraint<TEntity>>();

            this.RegisterTimestampConstraints();

            this.primaryKeyIndex =
                this.CreateUniqueIndex<TPrimaryKey>(
                    new RedBlackTreeIndexFactory<TEntity>(),
                    primaryKey);

            if (identitySpecification != null)
            {
                this.identityField = new IdentityField<TEntity>(identitySpecification, initialEntities);
            }

            this.Database.Core.RegisterEntityType<TEntity>();

            this.InitializeData(initialEntities);

            this.id = Interlocked.Increment(ref counter);
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the indexes of the table are changed.
        /// </summary>
        public event EventHandler IndexChanged;

        #endregion


        #region Lock management

        protected void AcquireWriteLock(Transaction transaction)
        {
            this.Database.ConcurrencyManager.AcquireTableWriteLock(this, transaction);
        }

        protected void ReleaseWriteLock(Transaction transaction)
        {
            this.Database.ConcurrencyManager.ReleaseTableWriteLock(this, transaction);
        }

        protected void AcquireReadLock(Transaction transaction)
        {
            this.Database.ConcurrencyManager.AcquireTableReadLock(this, transaction);
        }

        protected void ReleaseReadLock(Transaction transaction)
        {
            this.Database.ConcurrencyManager.ReleaseTableReadLock(this, transaction);
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">An entity that represents the property values of the new entity.</param>
        public void Insert(TEntity entity)
        {
            if (this.identityField != null)
            {
                this.identityField.Generate(entity);
            }

            try
            {
                using (var tran = Transaction.EnsureTransaction(this.Database))
                {
                    InsertCore(entity);

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }

        }

        protected abstract void InsertCore(TEntity entity);

        /// <summary>
        /// Updates the properties of an entity contained by the table.
        /// </summary>
        /// <param name="entity">An entity that represents the new property values.</param>
        public void Update(TEntity entity)
        {
            Update(primaryKeyIndex.KeyInfo.GetKey(entity), entity);
        }

        /// <summary>
        /// Updates the properties of an entity contained by the table.
        /// </summary>
        /// <param name="key">The primary key of the entity.</param>
        /// <param name="entity">An entity that represents the new property values.</param>
        public void Update(TPrimaryKey key, TEntity entity)
        {
            try
            {
                using (var tran = Transaction.EnsureTransaction(this.Database))
                {
                    UpdateCore(key, entity);

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

        int IBatchTable<TEntity>.Update(TableQuery<TEntity> query, Expression<Func<TEntity, TEntity>> updater)
        {
            updater = ExpressionHelper.ValidateAndCompleteUpdaterExpression(updater);
            Expression expression = ((IQueryable<TEntity>)query).Expression;

            try
            {
                using (var tran = Transaction.EnsureTransaction(this.Database))
                {
                    int result = UpdateCore(expression, updater);

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

        protected abstract void UpdateCore(TPrimaryKey key, TEntity entity);

        protected abstract int UpdateCore(Expression expression, Expression<Func<TEntity, TEntity>> updater);

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be deleted.</param>
        public void Delete(TEntity entity)
        {
            Delete(this.primaryKeyIndex.KeyInfo.GetKey(entity));
        }

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="key">The primary key of the entity to be deleted.</param>
        public void Delete(TPrimaryKey key)
        {
            try
            {
                using (var tran = Transaction.EnsureTransaction(this.Database))
                {
                    DeleteCore(key);

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

        int IBatchTable<TEntity>.Delete(TableQuery<TEntity> query)
        {
            Expression expression = ((IQueryable<TEntity>)query).Expression;

            try
            {
                using (var tran = Transaction.EnsureTransaction(this.Database))
                {
                    int result = DeleteCore(expression);

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

        protected abstract int DeleteCore(Expression expression);

        protected abstract void DeleteCore(TPrimaryKey key);

        #endregion

        #region Query

        /// <summary>
        /// Gets the number of entities contained in the table.
        /// </summary>
        public long Count
        {
            get
            {
                return this.LongCount();
            }
        }

        internal IEnumerable<TEntity> SelectAll()
        {
            return this.primaryKeyIndex.SelectAll();
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

        public IIndex<TEntity, TKey> CreateIndex<TKey>(
            IIndexFactory<TEntity> indexFactory,
            Expression<Func<TEntity, TKey>> key)
        {
            var index = indexFactory.CreateIndex(this, key);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        public IIndex<TEntity, TKey> CreateIndex<TKey>(
            IIndexFactory<TEntity> indexFactory,
            Expression<Func<TEntity, TKey>> key,
            IComparer<TKey> keyComparer)
        {
            var index = indexFactory.CreateIndex(this, key, keyComparer);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        public IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
            IIndexFactory<TEntity> indexFactory,
            Expression<Func<TEntity, TUniqueKey>> key)
        {
            var index = indexFactory.CreateUniqueIndex(this, key);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        public IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
            IIndexFactory<TEntity> indexFactory,
            Expression<Func<TEntity, TUniqueKey>> key,
            IComparer<TUniqueKey> keyComparer)
        {
            var index = indexFactory.CreateUniqueIndex(this, key, keyComparer);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        #endregion

        #region ITable Members

        /// <summary>
        /// Gets the indexes of the table.
        /// </summary>
        public IEnumerable<IIndex> Indexes
        {
            get { return indexes; }
        }

        /// <summary>
        /// Gets the primary key index of the table.
        /// </summary>
        public IUniqueIndex<TEntity, TPrimaryKey> PrimaryKeyIndex
        {
            get { return this.primaryKeyIndex; }
        }

        IIndex<TEntity> ITable<TEntity>.PrimaryKeyIndex
        {
            get { return this.primaryKeyIndex; }
        }

        IIndex ITable.PrimaryKeyIndex
        {
            get { return this.primaryKeyIndex; }
        }

        #endregion

        #region IReflectionTable Members

        void IReflectionTable.Update(object entity)
        {
            this.Update((TEntity)entity);
        }

        void IReflectionTable.Insert(object entity)
        {
            this.Insert((TEntity)entity);
        }

        void IReflectionTable.Delete(object entity)
        {
            this.Delete((TEntity)entity);
        }

        #endregion

        /// <summary>
        /// Returns a string that represents the table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Table<{0}>", this.EntityType.Name);
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

        protected void ApplyContraints(TEntity entity)
        {
            foreach (IConstraint<TEntity> constraint in this.constraints)
            {
                constraint.Apply(entity);
            }
        }


        private void InitializeData(IEnumerable<TEntity> initialEntities)
        {
            if (initialEntities == null)
            {
                return;
            }

            EntityPropertyCloner<TEntity> cloner = new EntityPropertyCloner<TEntity>();

            foreach (TEntity entity in initialEntities)
            {
                TEntity insert = this.Database.Core.CreateEntity<TEntity>();

                cloner.Clone(entity, insert);
                this.primaryKeyIndex.Insert(insert);
            }
        }
    }
}
