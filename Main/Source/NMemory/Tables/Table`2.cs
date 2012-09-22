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
        IBulkTable<TEntity>,
        IInitializableTable<TEntity>,
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
            IKeyInfo<TEntity, TPrimaryKey> primaryKey,
            IdentitySpecification<TEntity> identitySpecification)

            : this(database, identitySpecification)
        {
            this.primaryKeyIndex = CreateUniqueIndex(new DictionaryIndexFactory(), primaryKey);
        }

        private Table(
            IDatabase database,
            IdentitySpecification<TEntity> identitySpecification) 
            
            : base(database)
        {
            this.id = Interlocked.Increment(ref counter);
            this.VerifyType();

            this.indexes = new List<IIndex<TEntity>>();
            this.constraints = new List<IConstraint<TEntity>>();

            this.RegisterTimestampConstraints();

            if (identitySpecification != null)
            {
                this.identityField = new IdentityField<TEntity>(identitySpecification);
            }
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
            this.Database.DatabaseEngine.ConcurrencyManager.AcquireTableWriteLock(this, transaction);
        }

        protected void ReleaseWriteLock(Transaction transaction)
        {
            this.Database.DatabaseEngine.ConcurrencyManager.ReleaseTableWriteLock(this, transaction);
        }

        protected void AcquireReadLock(Transaction transaction)
        {
            this.Database.DatabaseEngine.ConcurrencyManager.AcquireTableReadLock(this, transaction);
        }

        protected void ReleaseReadLock(Transaction transaction)
        {
            this.Database.DatabaseEngine.ConcurrencyManager.ReleaseTableReadLock(this, transaction);
        }

        #endregion

        #region Manipulation


        public void Insert(TEntity entity)
        {
            this.Insert(entity, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public void Insert(TEntity entity, Transaction transaction)
        {
            if (this.identityField != null)
            {
                this.identityField.Generate(entity);
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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }

        }

        protected abstract void InsertCore(TEntity entity, Transaction transaction);

        public void Update(TEntity entity)
        {
            this.Update(entity, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public void Update(TEntity entity, Transaction transaction)
        {
            TPrimaryKey key = this.primaryKeyIndex.KeyInfo.SelectKey(entity);

            this.Update(key, entity, transaction);
        }

        public void Update(TPrimaryKey key, TEntity entity)
        {
            this.Update(key, entity, Transaction.TryGetAmbientEnlistedTransaction());
        }

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

        protected abstract void UpdateCore(TPrimaryKey key, TEntity entity, Transaction transaction);

        protected abstract IEnumerable<TEntity> UpdateCore(Expression expression, Expression<Func<TEntity, TEntity>> updater, Transaction transaction);

        
        public void Delete(TEntity entity)
        {
            this.Delete(entity, Transaction.TryGetAmbientEnlistedTransaction());
        }

        public void Delete(TEntity entity, Transaction transaction)
        {
            TPrimaryKey key = this.primaryKeyIndex.KeyInfo.SelectKey(entity);

            this.Delete(key, transaction);
        }

        public void Delete(TPrimaryKey key)
        {
            this.Delete(key, Transaction.TryGetAmbientEnlistedTransaction());
        }

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

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
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

        protected abstract int DeleteCore(Expression expression, Transaction transaction);

        protected abstract void DeleteCore(TPrimaryKey key, Transaction transaction);

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
            IIndexFactory indexFactory,
            Expression<Func<TEntity, TKey>> key)
        {
            var index = indexFactory.CreateIndex(this, key);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        public IIndex<TEntity, TKey> CreateIndex<TKey>(
            IIndexFactory indexFactory,
            IKeyInfo<TEntity, TKey> keyInfo)
        {
            var index = indexFactory.CreateIndex(this, keyInfo);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        public IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
            IIndexFactory indexFactory,
            Expression<Func<TEntity, TUniqueKey>> key)
        {
            var index = indexFactory.CreateUniqueIndex(this, key);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

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

        IUniqueIndex<TEntity> ITable<TEntity>.PrimaryKeyIndex
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

        public void AddConstraint(IConstraint<TEntity> constraint)
        {
            this.constraints.Add(constraint);
        }

        void IInitializableTable<TEntity>.Initialize(IEnumerable<TEntity> initialEntities)
        {
            if (this.primaryKeyIndex.Count > 0)
            {
                throw new InvalidOperationException();
            }

            this.InitializeData(initialEntities);

            if (this.identityField != null)
            {
                this.identityField.InitializeBasedOnData(this.primaryKeyIndex.SelectAll());
            }
        }

        /// <summary>
        /// Returns a string that represents the table.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Table<{0}>", this.EntityType.Name);
        }

        protected void ApplyContraints(TEntity entity)
        {
            foreach (IConstraint<TEntity> constraint in this.constraints)
            {
                constraint.Apply(entity);
            }
        }

        protected virtual TEntity CreateStoredEntity()
        {
            return Activator.CreateInstance<TEntity>();
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

        private void InitializeData(IEnumerable<TEntity> initialEntities)
        {
            if (initialEntities == null)
            {
                return;
            }

            EntityPropertyCloner<TEntity> cloner = new EntityPropertyCloner<TEntity>();

            foreach (TEntity entity in initialEntities)
            {
                TEntity insert = this.CreateStoredEntity();

                cloner.Clone(entity, insert);
                this.primaryKeyIndex.Insert(insert);
            }
        }

        
    }
}
