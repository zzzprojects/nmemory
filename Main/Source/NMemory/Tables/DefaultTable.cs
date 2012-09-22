using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common.Visitors;
using NMemory.Execution;
using NMemory.Indexes;
using NMemory.Modularity;
using NMemory.Transactions;
using NMemory.Transactions.Logs;

namespace NMemory.Tables
{
    internal class DefaultTable<TEntity, TPrimaryKey> : 
        Table<TEntity, TPrimaryKey> 
        where TEntity : class
    {
        private EntityPropertyCloner<TEntity> cloner;
        private EntityPropertyChangeDetector<TEntity> changeDetector;

        internal DefaultTable(
            IDatabase database,
            IKeyInfo<TEntity, TPrimaryKey> primaryKey,
            IdentitySpecification<TEntity> identitySpecification)

            : base(database, primaryKey, identitySpecification)
        {
            this.changeDetector = new EntityPropertyChangeDetector<TEntity>();
            this.cloner = new EntityPropertyCloner<TEntity>();
        }

        #region Insert

        protected override void InsertCore(TEntity entity, Transaction transaction)
        {
            TEntity storedEntity = this.CreateStoredEntity();
            this.cloner.Clone(entity, storedEntity);

            this.ApplyContraints(storedEntity);

            // Find referred relations
            List<IRelation> referredRelations = new List<IRelation>();
            this.FindRelations(this.Indexes, null, referredRelations);

            // Find related tables
            List<ITable> relatedTables = this.GetRelatedTables(null, referredRelations).ToList();

            // Lock table
            this.AcquireWriteLock(transaction);
            // Lock the related tables
            this.LockRelatedTables(transaction, relatedTables);

            ITransactionLog log = this.Database.DatabaseEngine.TransactionHandler.GetTransactionLog(transaction);
            int logPosition = log.CurrentPosition;

            try
            {
                // Validate referred relations
                this.ValidateReferredRelations(referredRelations, new TEntity[] { storedEntity });

                // Get referred entities
                HashSet<object> referredEntities = new HashSet<object>();
                this.FindRelatedEntities(new TEntity[] { storedEntity }, null, referredRelations, null, referredEntities);

                transaction.EnterAtomicSection();

                try
                {
                    foreach (IIndex<TEntity> index in this.Indexes)
                    {
                        index.Insert(storedEntity);
                        log.WriteIndexInsert(index, storedEntity);
                    }

                    // Copy back the modifications
                    this.cloner.Clone(storedEntity, entity);
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
                this.ReleaseWriteLock(transaction);
            }
        }

        #endregion

        #region Update

        protected override void UpdateCore(TPrimaryKey key, TEntity entity, Transaction transaction)
        {
            this.AcquireWriteLock(transaction);

            try
            {
                TEntity storedEntity = this.PrimaryKeyIndex.GetByUniqueIndex(key);
                Expression<Func<TEntity, TEntity>> updater = CreateSingleEntityUpdater(entity, storedEntity);

                this.UpdateCore(new TEntity[] { storedEntity }, updater, transaction);

                // Copy back the modifications
                this.cloner.Clone(storedEntity, entity);
            }
            finally
            {
                this.ReleaseWriteLock(transaction);
            }
        }

        protected override IEnumerable<TEntity> UpdateCore(Expression expression, Expression<Func<TEntity, TEntity>> updater, Transaction transaction)
        {
            // Optimize and compile the query
            List<TEntity> result = null;

            this.AcquireWriteLock(transaction);

            try
            {
                result = this.QueryEntities(expression, transaction);

                this.UpdateCore(result, updater, transaction);
            }
            finally
            {
                this.ReleaseWriteLock(transaction);
            }

            return this.CloneEntities(result);
        }

        private void UpdateCore(IList<TEntity> storedEntities, Expression<Func<TEntity, TEntity>> updater, Transaction transaction)
        {
            Func<TEntity, TEntity> updaterFunc = updater.Compile();
            IList<TEntity> updated = new List<TEntity>(storedEntities.Count);

            PropertyInfo[] changes = FindPossibleChanges(updater);

            // It is possible, that there are no changes
            if (changes.Length == 0)
            {
                return;
            }

            // Determine the potential indexes
            List<IIndex<TEntity>> potentialIndexes = new List<IIndex<TEntity>>();
            foreach (IIndex<TEntity> index in this.Indexes)
            {
                if (index.KeyInfo.EntityKeyMembers.Any(x => changes.Contains(x)))
                {
                    potentialIndexes.Add(index);
                }
            }

            // Find relations
            List<IRelation> referringRelations = new List<IRelation>();
            List<IRelation> referredRelations = new List<IRelation>();
            this.FindRelations(potentialIndexes, referringRelations, referredRelations);

            // Find related tables to lock
            List<ITable> relatedTables = this.GetRelatedTables(referringRelations, referredRelations).ToList();

            // Lock related tables
            this.LockRelatedTables(transaction, relatedTables);

            // Find related entities
            HashSet<object> referringEntities = new HashSet<object>();
            HashSet<object> referredEntities = new HashSet<object>();
            this.FindRelatedEntities(storedEntities, referringRelations, referredRelations, referringEntities, referredEntities);

            // Get the transaction log
            ITransactionLog log = this.Database.DatabaseEngine.TransactionHandler.GetTransactionLog(transaction);
            int logPosition = log.CurrentPosition;

            transaction.EnterAtomicSection();

            try
            {
                // Delete invalid index records
                for (int i = 0; i < storedEntities.Count; i++)
                {
                    TEntity storedEntity = storedEntities[i];

                    foreach (IIndex<TEntity> index in potentialIndexes)
                    {
                        index.Delete(storedEntity);
                        log.WriteIndexDelete(index, storedEntity);
                    }
                }

                // Modify entity properties
                for (int i = 0; i < storedEntities.Count; i++)
                {
                    TEntity storedEntity = storedEntities[i];

                    // Create backup
                    TEntity backup = Activator.CreateInstance<TEntity>();
                    this.cloner.Clone(storedEntity, backup);
                    TEntity newEntity = updaterFunc.Invoke(storedEntity);

                    // Apply contraints on the entity
                    this.ApplyContraints(newEntity);

                    // Update entity
                    this.cloner.Clone(newEntity, storedEntity);
                    log.WriteEntityUpdate(this.cloner, storedEntity, backup);
                }

                // Insert to index again
                for (int i = 0; i < storedEntities.Count; i++)
                {
                    TEntity storedEntity = storedEntities[i];

                    foreach (IIndex<TEntity> index in potentialIndexes)
                    {
                        index.Insert(storedEntity);
                        log.WriteIndexInsert(index, storedEntity);
                    }
                }

                // Validate referred relations
                this.ValidateReferredRelations(referredRelations, storedEntities);

                // Validate referring relations
                this.ValidateReferringRelations(referringRelations, referringEntities);
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

        #endregion

        #region Delete

        protected override int DeleteCore(Expression expression, Transaction transaction)
        {
            List<TEntity> result = null;

            this.AcquireWriteLock(transaction);

            try
            {
                result = this.QueryEntities(expression, transaction);

                this.DeleteCore(result, transaction);
            }
            finally
            {
                this.ReleaseWriteLock(transaction);
            }

            return result != null ? result.Count : 0;
        }

        protected override void DeleteCore(TPrimaryKey key, Transaction transaction)
        {
            this.AcquireWriteLock(transaction);

            try
            {
                TEntity storedEntity = this.PrimaryKeyIndex.GetByUniqueIndex(key);

                this.DeleteCore(new TEntity[] { storedEntity }, transaction);
            }
            finally
            {
                this.ReleaseWriteLock(transaction);
            }
        }

        private void DeleteCore(IList<TEntity> storedEntities, Transaction transaction)
        {
            // Find relations
            List<IRelation> referringRelations = new List<IRelation>();
            List<IRelation> referredRelations = new List<IRelation>();
            this.FindRelations(this.Indexes, referringRelations, referredRelations);

            // Find related tables to lock
            List<ITable> relatedTables = this.GetRelatedTables(referringRelations, referredRelations).ToList();

            // Lock related tables
            this.LockRelatedTables(transaction, relatedTables);

            // Find related entities
            HashSet<object> referringEntities = new HashSet<object>();
            HashSet<object> referredEntities = new HashSet<object>();
            this.FindRelatedEntities(storedEntities, referringRelations, referredRelations, referringEntities, referredEntities);

            ITransactionLog log = this.Database.DatabaseEngine.TransactionHandler.GetTransactionLog(transaction);
            int logPosition = log.CurrentPosition;

            transaction.EnterAtomicSection();

            try
            {
                // Delete invalid index records
                for (int i = 0; i < storedEntities.Count; i++)
                {
                    TEntity storedEntity = storedEntities[i];

                    foreach (IIndex<TEntity> index in this.Indexes)
                    {
                        index.Delete(storedEntity);
                        log.WriteIndexDelete(index, storedEntity);
                    }
                }

                // Validate referring relations
                this.ValidateReferringRelations(referringRelations, referringEntities);
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

        #endregion

        private List<TEntity> QueryEntities(Expression expression, Transaction transaction)
        {
            IExecutionPlan<IEnumerable<TEntity>> plan = this.Database.DatabaseEngine.Compiler.Compile<IEnumerable<TEntity>>(expression);
            
            // Find the remaining tables of the query
            ITable[] tables = TableSearchVisitor.FindTables(expression).Except(new ITable[] { this }).ToArray();
            
            IExecutionContext context = new ExecutionContext(this.Database, transaction, tables);

            // Lock these tables
            for (int i = 0; i < tables.Length; i++)
            {
                this.Database.DatabaseEngine.ConcurrencyManager.AcquireTableReadLock(tables[i], transaction);
            }

            try
            {
                return plan.Execute(context).Distinct().ToList();
            }
            finally
            {
                // Release the tables locks
                for (int i = 0; i < tables.Length; i++)
                {
                    this.Database.DatabaseEngine.ConcurrencyManager.AcquireTableReadLock(tables[i], transaction);
                }
            }
        }

        private IEnumerable<ITable> GetRelatedTables(IEnumerable<IRelation> referringRelations, IEnumerable<IRelation> referredRelations)
        {
            return
                (referringRelations ?? Enumerable.Empty<IRelation>()).Select(x => x.ForeignTable)
                .Concat((referredRelations ?? Enumerable.Empty<IRelation>()).Select(x => x.PrimaryTable))
                .Distinct()
                .Except(new ITable[] { this }); // This table is already locked
        }

        private void LockRelatedTables(Transaction transaction, IEnumerable<ITable> relatedTables)
        {
            foreach (ITable table in relatedTables)
            {
                this.Database.DatabaseEngine.ConcurrencyManager.AcquireRelatedTableLock(table, transaction);
            }
        }

        private void FindRelations(IEnumerable<IIndex> indexes, ICollection<IRelation> referringRelations, ICollection<IRelation> referredRelations)
        {
            foreach (IIndex index in indexes)
            {
                if (referringRelations != null)
                {
                    foreach (IRelation relation in this.Database.Tables.GetReferringRelations(index))
                    {
                        referringRelations.Add(relation);
                    }
                }

                if (referredRelations != null)
                {
                    foreach (IRelation relation in this.Database.Tables.GetReferedRelations(index))
                    {
                        referredRelations.Add(relation);
                    }
                }
            }
        }

        private void FindRelatedEntities(
            IList<TEntity> storedEntities, 
            IEnumerable<IRelation> referringRelations, 
            IEnumerable<IRelation> referredRelations, 
            
            HashSet<object> referringEntities, 
            HashSet<object> referredEntities)
        {
            for (int i = 0; i < storedEntities.Count; i++)
            {
                TEntity storedEntity = storedEntities[i];

                if (referringRelations != null && referringEntities != null)
                {
                    foreach (IRelation relation in referringRelations)
                    {
                        foreach (object entity in relation.GetReferringEntities(storedEntity))
                        {
                            referringEntities.Add(entity);
                        }
                    }
                }

                if (referredRelations != null && referredEntities != null)
                {
                    foreach (IRelation relation in referredRelations)
                    {
                        foreach (object entity in relation.GetReferredEntities(storedEntity))
                        {
                            referredEntities.Add(entity);
                        }
                    }
                }
            }
        }

        private void ValidateReferredRelations(IList<IRelation> referredRelations, IList<TEntity> storedEntities)
        {
            if (referredRelations.Count == 0)
            {
                return;
            }

            for (int i = 0; i < storedEntities.Count; i++)
            {
                TEntity storedEntity = storedEntities[i];

                for (int j = 0; j < referredRelations.Count; j++)
                {
                    referredRelations[j].ValidateEntity(storedEntities[i]);
                }
            }
        }

        private void ValidateReferringRelations(IList<IRelation> referringRelations, HashSet<object> referringEntities)
        {
            if (referringEntities.Count == 0)
            {
                return;
            }

            foreach (object referringEntity in referringEntities)
            {
                for (int i = 0; i < referringRelations.Count; i++)
                {
                    referringRelations[i].ValidateEntity(referringEntity);
                }
            }
        }

        private Expression<Func<TEntity, TEntity>> CreateSingleEntityUpdater(TEntity entity, TEntity storedEntity)
        {
            List<PropertyInfo> changes = this.changeDetector.GetChanges(storedEntity, entity);

            PropertyInfo[] properties = typeof(TEntity).GetProperties();
            MemberBinding[] bindings = new MemberBinding[properties.Length];
            ParameterExpression exprParam = Expression.Parameter(typeof(TEntity));

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                Expression source = null;

                // Check if the property was changed
                if (changes.Contains(property))
                {
                    // If so, use the new value
                    source = Expression.Constant(entity);
                }
                else
                {
                    // Elso, use the old value
                    source = exprParam;
                }

                bindings[i] = Expression.Bind(property, Expression.Property(source, property));
            }

            MemberInitExpression updater = Expression.MemberInit(Expression.New(typeof(TEntity)), bindings);
            return Expression.Lambda<Func<TEntity, TEntity>>(updater, exprParam);
        }

        private PropertyInfo[] FindPossibleChanges(Expression<Func<TEntity, TEntity>> updater)
        {
            MemberInitExpression creator = updater.Body as MemberInitExpression;
            List<PropertyInfo> changes = new List<PropertyInfo>();

            foreach (MemberAssignment assign in creator.Bindings)
            {
                MemberExpression memberRead = assign.Expression as MemberExpression;

                // Check if the member is not assigned with the same member
                if (memberRead == null || memberRead.Member.Name != assign.Member.Name)
                {
                    changes.Add(assign.Member as PropertyInfo);
                    continue;
                }

                // Check if the source is not the parameter
                if (!(memberRead.Expression is ParameterExpression))
                {
                    changes.Add(assign.Member as PropertyInfo);
                    continue;
                }

            }

            return changes.ToArray();
        }

        private List<TEntity> CloneEntities(IList<TEntity> entities)
        {
            List<TEntity> result = new List<TEntity>();

            for (int i = 0; i < entities.Count; i++)
            {
                TEntity entity = Activator.CreateInstance<TEntity>();
                this.cloner.Clone(entities[i], entity);

                result.Add(entity);
            }

            return result;
        }
    }
}
