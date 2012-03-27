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
using NMemory.Statistics;
using NMemory.Transactions;
using NMemory.Transactions.Logs;
using NMemory.Modularity;

namespace NMemory.Tables
{
    public class Table<TEntity, TPrimaryKey> : 
        
        TableQuery<TEntity>, 

        ITable<TEntity, TPrimaryKey>, 
        IQueryable<TEntity>, 
        IReflectionTable

        where TEntity : class
    {

        #region Members

        private IdentityField<TEntity> identityField;

        private EntityPropertyCloner<TEntity> cloner;
        private EntityPropertyChangeDetector<TEntity> changeDetector;

        private UniqueIndex<TEntity, TPrimaryKey> primaryKeyIndex;
        private IList<IIndex<TEntity>> indexes;
        private IList<IConstraint<TEntity>> constraints;

        private Histogram<TEntity> histogram;

        

        private static int counter;
        private int id;

        #endregion

        #region Constructor

        internal Table(
            Database database,
            Expression<Func<TEntity, TPrimaryKey>> primaryKey, 
            
            IdentitySpecification<TEntity> identitySpecification = null, 
            IEnumerable<TEntity> initialEntities = null) 
            
            : base(database)
        {
            this.VerifyType();

            this.indexes = new List<IIndex<TEntity>>();
            this.constraints = new List<IConstraint<TEntity>>();

            this.RegisterTimestampConstraints();

            this.changeDetector = new EntityPropertyChangeDetector<TEntity>();
            this.cloner = new EntityPropertyCloner<TEntity>();
            
            this.primaryKeyIndex = 
                this.CreateUniqueIndex<TPrimaryKey>(
                    new RedBlackTreeIndexFactory<TEntity>(),
                    primaryKey);

            this.histogram = new Histogram<TEntity>( this );

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

        /// <summary>
        /// Occurs before a new entity is inserted.
        /// </summary>
        public event EventHandler<EntityEventArgs<TEntity>> EntityInserting;

        /// <summary>
        /// Occurs after a new entity is inserted.
        /// </summary>
        public event EventHandler<EntityEventArgs<TEntity>> EntityInserted;

        /// <summary>
        /// Occurs before an entity is updated.
        /// </summary>
        public event EventHandler<EntityUpdateEventArgs<TEntity>> EntityUpdating;

        /// <summary>
        /// Occurs after an entity is updated.
        /// </summary>
        public event EventHandler<EntityUpdateEventArgs<TEntity>> EntityUpdated;

        /// <summary>
        /// Occurs before an entity is deleted.
        /// </summary>
        public event EventHandler<EntityEventArgs<TEntity>> EntityDeleting;

        /// <summary>
        /// Occurs after an entity is deleted.
        /// </summary>
        public event EventHandler<EntityEventArgs<TEntity>> EntityDeleted;

        #endregion


        #region Lock management

        private void AcquireWriteLock(Transaction transaction)
        {
            this.Database.ConcurrencyManager.AcquireTableWriteLock(this, transaction);
        }

        private void ReleaseWriteLock(Transaction transaction)
        {
            this.Database.ConcurrencyManager.ReleaseTableWriteLock(this, transaction);
        }

        private void AcquireReadLock(Transaction transaction)
        {
            this.Database.ConcurrencyManager.AcquireTableReadLock(this, transaction);
        }

        private void ReleaseReadLock(Transaction transaction)
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

            // PreEvent
            this.OnItemInserting( entity );

            try
            {
                using (var tran = base.EnsureTransaction())
                {
                    InsertCore(entity);

                    tran.Complete();
                }

                // PostEvent
                this.OnItemInserted(entity);
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

        private void InsertCore(TEntity entity)
        {
            this.ApplyContraints(entity);

            TEntity storedEntity = this.Database.Core.CreateEntity<TEntity>();
            this.cloner.Clone(entity, storedEntity);

            Transaction transaction = Transaction.Current;

            if (transaction == null)
            {
                throw new InvalidOperationException("No transaction");
            }

            this.Database.TransactionHandler.EnsureSubscription(transaction);
            TransactionLog log = this.Database.TransactionHandler.GetTransactionLog(transaction);
            int logPosition = log.CurrentPosition;

            this.AcquireWriteLock(transaction);
            transaction.EnterAtomicSection();

            try
            {
                foreach (IIndex<TEntity> index in this.indexes)
                {
                    index.Insert(storedEntity);

                    log.WriteIndexInsert(index, storedEntity);
                }
            }
            catch
            {
                log.RollbackTo(logPosition);

                throw;
            }
            finally
            {
                transaction.ExitAtomicSection();
                this.ReleaseWriteLock(transaction);
            }
        }

        /// <summary>
        /// Updates the properties of an entity contained by the table.
        /// </summary>
        /// <param name="originalEntity">An entity that contains the primary key of the entity to be updated.</param>
        /// <param name="entity">An entity that represents the new property values.</param>
        public void Update(TEntity originalEntity, TEntity entity)
        {
            // PreEvent
            this.OnItemUpdating(originalEntity, entity);

            try
            {
                using (var tran = base.EnsureTransaction())
                {
                    UpdateCore(originalEntity, entity);

                    tran.Complete();
                }

                // PostEvent
                this.OnItemUpdated(originalEntity, entity);
            }
            catch (System.Transactions.TransactionAbortedException)
            {
                throw;
            }
            catch(NMemoryException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new NMemoryException(ErrorCodes.GenericError, ex);
            }
        }

        private void UpdateCore(TEntity oldEntity, TEntity newEntity)
        {
            this.ApplyContraints(newEntity);

            Transaction transaction = Transaction.Current;

            if (transaction == null)
            {
                throw new InvalidOperationException();
            }

            this.Database.TransactionHandler.EnsureSubscription(transaction);
            TransactionLog log = this.Database.TransactionHandler.GetTransactionLog(transaction);
            int logPosition = log.CurrentPosition;

            HashSet<ITable> tables = new HashSet<ITable>();
            
            tables.Add(this);
            this.Database.ConcurrencyManager.AcquireTableWriteLock(this, transaction);

            try
            {
                TPrimaryKey key = primaryKeyIndex.Key(oldEntity);
                TEntity storedEntity = primaryKeyIndex.GetByUniqueIndex(key);

                if (storedEntity == null)
                {
                    return;
                }

                List<PropertyInfo> changedProperties = this.changeDetector.GetChanges(storedEntity, newEntity);
                List<IIndex<TEntity>> detectedIndexes = new List<IIndex<TEntity>>();

                foreach (IIndex<TEntity> index in this.indexes)
                {
                    if (index.KeyInfo.KeyMembers.Any(x => changedProperties.Contains(x)))
                    {
                        detectedIndexes.Add(index);
                    }
                }

                // If there is no change, leave
                if (changedProperties.Count == 0)
                {
                    return;
                }

                // Lock related tables
                foreach (IIndex<TEntity> index in detectedIndexes)
                {
                    List<ITable> indexTables = new List<ITable>();

                    if (index == this.PrimaryKeyIndex)
                    {
                        indexTables.AddRange(this.Database.Tables.GetReferringRelations(this).Select(x => x.ForeignTable));
                    }
                    else
                    {
                        indexTables.AddRange(this.Database.Tables.GetReferedRelations(index).Select(x => x.PrimaryTable));
                    }

                    foreach (ITable table in indexTables)
                    {
                        if (tables.Add(table))
                        {
                            this.Database.ConcurrencyManager.AcquireTableWriteLock(table, transaction);
                        }
                    }
                }

                transaction.EnterAtomicSection();

                try
                {
                    // Delete invalid index records
                    foreach (IIndex<TEntity> index in detectedIndexes)
                    {
                        index.Delete(storedEntity);
                        log.WriteIndexDelete(index, storedEntity);
                    }

                    // Create backup
                    TEntity backup = Activator.CreateInstance<TEntity>();
                    this.cloner.Clone(storedEntity, backup);

                    // Update entity
                    this.cloner.Clone(newEntity, storedEntity);
                    log.WriteEntityUpdate(this.cloner, storedEntity, backup);

                    // Insert new index records
                    foreach (IIndex<TEntity> index in detectedIndexes)
                    {
                        index.Insert(storedEntity);
                        log.WriteIndexInsert(index, storedEntity);
                    }
                }
                catch
                {
                    log.RollbackTo(logPosition);
                    throw;
                }
                finally
                {
                    transaction.ExitAtomicSection();
                }
            }
            finally
            {
                // Release all locks
                foreach (ITable table in tables)
                {
                    this.Database.ConcurrencyManager.ReleaseTableWriteLock(table, transaction);
                }
            }
        }

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">An entity that contains the primary key of the entity to be deleted.</param>
        public void Delete(TEntity entity)
        {
            // PreEvent
            this.OnItemDeleting(entity);

            try
            {
                using (var tran = base.EnsureTransaction())
                {
                    DeleteCore(entity);

                    tran.Complete();
                }

                // PostEvent
                this.OnItemDeleted(entity);
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

        private void DeleteCore(TEntity entity)
        {
            Transaction transaction = Transaction.Current;

            if (transaction == null)
            {
                throw new InvalidOperationException("No transaction");
            }

            this.Database.TransactionHandler.EnsureSubscription(transaction);
            TransactionLog log = this.Database.TransactionHandler.GetTransactionLog(transaction);
            int logPosition = log.CurrentPosition;

            HashSet<ITable> tables = new HashSet<ITable>();

            tables.Add(this);
            this.Database.ConcurrencyManager.AcquireTableWriteLock(this, transaction);

            try
            {
                TPrimaryKey key = primaryKeyIndex.Key(entity);
                TEntity storedEntity = primaryKeyIndex.GetByUniqueIndex(key);

                if (storedEntity == null)
                {
                    return;
                }

                IList<ITable> foreignTables = this.Database.Tables.GetReferringRelations(this).Select(x => x.ForeignTable).ToList();

                // Lock referenced tables
                foreach (ITable table in foreignTables)
                {
                    if (tables.Add(table))
                    {
                        this.Database.ConcurrencyManager.AcquireTableWriteLock(table, transaction);
                    }
                }

                transaction.EnterAtomicSection();

                try
                {
                    foreach (IIndex<TEntity> index in this.indexes)
                    {
                        index.Delete(storedEntity);

                        log.WriteIndexDelete(index, storedEntity);
                    }
                }
                catch
                {
                    log.RollbackTo(logPosition);
                    throw;
                }
                finally
                {
                    transaction.ExitAtomicSection();
                }
            }
            finally
            {
                // Release all locks
                foreach (ITable table in tables)
                {
                    this.Database.ConcurrencyManager.ReleaseTableWriteLock(table, transaction);
                }
            }

        }

        #endregion
        
        #region Query

        /// <summary>
        /// Gets the number of entities contained in the table.
        /// </summary>
        public long Count
        {
            get
            {
                using (var tran = base.EnsureTransaction())
                {
                    long result = this.GetTransactedCount();

                    tran.Complete();
                    return result;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the entities of the table.
        /// </summary>
        /// <returns>An IEnumerator object </returns>
        public override IEnumerator<TEntity> GetEnumerator()
        {
            IQueryEnumeratorFactory enumeratorFactory = this.Database.Executor.CreateQueryEnumeratorFactory();

            return enumeratorFactory.Create<TEntity>(this, this.EnsureTransaction());
        }

        internal IEnumerable<TEntity> SelectAll()
        {
            return this.primaryKeyIndex.SelectAll();
        }

        internal long GetCount()
        {
            return this.primaryKeyIndex.Count;
        }

        private long GetTransactedCount()
        {
            Transaction transaction = Transaction.Current;

            AcquireReadLock(transaction);

            try
            {
                return GetCount();
            }
            finally
            {
                ReleaseReadLock(transaction);
            }
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

        private void OnItemInserting(TEntity t)
        {
            if (this.EntityInserting != null)
            {
                this.EntityInserting(this, new EntityEventArgs<TEntity>(t));
            }
        }

        private void OnItemInserted(TEntity t)
        {
            if (this.EntityInserted != null)
            {
                this.EntityInserted(this, new EntityEventArgs<TEntity>(t));
            }
        }

        private void OnItemDeleting(TEntity t)
        {
            if (this.EntityDeleting != null)
            {
                this.EntityDeleting(this, new EntityEventArgs<TEntity>(t));
            }
        }

        private void OnItemDeleted(TEntity t)
        {
            if (this.EntityDeleted != null)
            {
                this.EntityDeleted(this, new EntityEventArgs<TEntity>(t));
            }
        }

        private void OnItemUpdating(TEntity itemOld, TEntity itemUpdated)
        {
            if (this.EntityUpdating != null)
            {
                this.EntityUpdating(this, new EntityUpdateEventArgs<TEntity>(itemOld, itemUpdated));
            }
        }



        private void OnItemUpdated(TEntity itemOld, TEntity itemUpdated)
        {
            if (this.EntityUpdated != null)
            {
                this.EntityUpdated(this, new EntityUpdateEventArgs<TEntity>(itemOld, itemUpdated));
            }
        }

        #endregion

        #region Index factory methods

        public IIndex<TEntity, TKey> CreateIndex<TKey>(
            IIndexFactory<TEntity> indexFactory,
            Expression<Func<TEntity, TKey>> key)
        {
            // Check if a custom comparer is needed
            IComparer<TKey> comparer;

            if (CreateComparerIfNeeded<TKey>(out comparer))
            {
                // TODO: Move this somewhere else
                return CreateIndex(indexFactory, key, comparer);
            }

            var index = indexFactory.CreateIndex(this, key);

            this.indexes.Add( index );
            this.OnIndexChanged();

            return index;
        }

        public IIndex<TEntity, TKey> CreateIndex<TKey>(
            IIndexFactory<TEntity> indexFactory,
            Expression<Func<TEntity, TKey>> key,
            IComparer<TKey> keyComparer)
        {
            var index = indexFactory.CreateIndex(this, key, keyComparer);

            this.indexes.Add( index );
            this.OnIndexChanged();

            return index;
        }

        public UniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
            IIndexFactory<TEntity> indexFactory,
            Expression<Func<TEntity, TUniqueKey>> key)
        {
            // Check if a custom comparer is needed
            IComparer<TUniqueKey> comparer;
            if (CreateComparerIfNeeded<TUniqueKey>(out comparer))
            {
                // TODO: Move this somewhere else
                return CreateUniqueIndex(indexFactory, key, comparer);
            }

            var index = indexFactory.CreateUniqueIndex(this, key);

            this.indexes.Add(index);
            this.OnIndexChanged();

            return index;
        }

        public UniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TUniqueKey>(
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

        public IStatistics Statistics
        {
            get { return this.histogram; }
        }

        #endregion

        #region IReflectionTable Members

        void IReflectionTable.Update( object originalEntity, object entity )
        {
            this.Update((TEntity)originalEntity, (TEntity)entity);
        }

        void IReflectionTable.Insert( object entity )
        {
            this.Insert((TEntity)entity);
        }

        void IReflectionTable.Delete( object entity )
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

        private void ApplyContraints(TEntity entity)
        {
            foreach (IConstraint<TEntity> constraint in this.constraints)
            {
                constraint.Apply(entity);
            }
        }

        private bool CreateComparerIfNeeded<TKey>(out IComparer<TKey> comparer)
        {
            comparer = null;

            if (TypeSystem.IsAnonymousType(typeof(TKey)))
            {
                comparer = new AnonymousTypeComparer<TKey>();
                return true;
            }

            return false;
        }

        private void InitializeData(IEnumerable<TEntity> initialEntities)
        {
            if (initialEntities != null)
            {
                foreach (TEntity entity in initialEntities)
                {
                    TEntity insert = this.Database.Core.CreateEntity<TEntity>();

                    this.cloner.Clone(entity, insert);
                    this.primaryKeyIndex.Insert(insert);
                }
            }
        } 
    }
}
