// ----------------------------------------------------------------------------------
// <copyright file="CommandExecutor.cs" company="NMemory Team">
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

namespace NMemory.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NMemory.Common;
    using NMemory.Execution.Primitives;
    using NMemory.Indexes;
    using NMemory.Modularity;
    using NMemory.Services.Contracts;
    using NMemory.Tables;
    using NMemory.Transactions.Logs;
    using NMemory.Utilities;

    public class CommandExecutor : ICommandExecutor
    {
        private IDatabase database;

        public void Initialize(IDatabase database)
        {
            this.database = database;
        }

        #region Query

        public IEnumerator<T> ExecuteQuery<T>(
            IExecutionPlan<IEnumerable<T>> plan, 
            IExecutionContext context)
        {
            ITable[] tables = TableLocator.FindAffectedTables(context.Database, plan);

            return ExecuteQuery(plan, context, tables, cloneEntities: true);
        }

         private IEnumerator<T> ExecuteQuery<T>(
            IExecutionPlan<IEnumerable<T>> plan, 
            IExecutionContext context,
            ITable[] tablesToLock,
            bool cloneEntities)
        {
            ITable[] tables = TableLocator.FindAffectedTables(context.Database, plan);

            Action<T, T> cloner = null;
            if (!NMemoryManager.DisableObjectCloning && cloneEntities && this.database.Tables.IsEntityType<T>())
            {
                cloner = context
                    .GetService<IEntityService>()
                    .CloneProperties<T>;
            }

            LinkedList<T> result = new LinkedList<T>();

            for (int i = 0; i < tablesToLock.Length; i++)
            {
                this.AcquireReadLock(tablesToLock[i], context);
            }

            IEnumerable<T> query = plan.Execute(context);

            try
            {
                foreach (T item in query)
                {
                    if (cloner != null && item != null)
                    {
                        T resultEntity = Activator.CreateInstance<T>();
                        cloner(item, resultEntity);

                        result.AddLast(resultEntity);
                    }
                    else
                    {
                        result.AddLast(item);
                    }
                }
            }
            finally
            {
                for (int i = 0; i < tablesToLock.Length; i++)
                {
                    this.ReleaseReadLock(tablesToLock[i], context);
                }
            }

            return result.GetEnumerator();
        }

        public T ExecuteQuery<T>(
            IExecutionPlan<T> plan, 
            IExecutionContext context)
        {
            ITable[] tables = TableLocator.FindAffectedTables(context.Database, plan);

            for (int i = 0; i < tables.Length; i++)
            {
                this.AcquireReadLock(tables[i], context);
            }

            Action<T, T> cloner = context
                .GetService<IEntityService>()
                .CloneProperties<T>;

            try
            {
                var result = plan.Execute(context);

                if (!NMemoryManager.DisableObjectCloning && this.database.Tables.IsEntityType<T>() && result != null)
                {
                    T resultEntity = Activator.CreateInstance<T>();
                    cloner(result, resultEntity);

                    result = resultEntity;
                }

                return result;
            }
            finally
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    this.ReleaseReadLock(tables[i], context);
                }
            }
        }

        #endregion

        #region Insert
        
        public void ExecuteInsert<T>(T entity, IExecutionContext context) 
            where T : class
        {
            var helper = new ExecutionHelper(this.Database);
            var table = this.Database.Tables.FindTable<T>();
			 
			table.Contraints.Apply(entity, context, table);

            // Find referred relations
            // Do not add referring relations!
            RelationGroup relations = helper.FindRelations(table.Indexes, referring: false);

            // Acquire locks
            this.AcquireWriteLock(table, context);
            this.LockRelatedTables(relations, context, table);

            try
            {
                // Validate the inserted record
                helper.ValidateForeignKeys(relations.Referred, new[] { entity });

                using (AtomicLogScope logScope = this.StartAtomicLogOperation(context))
                {
                    foreach (IIndex<T> index in table.Indexes)
                    {
                        index.Insert(entity);
                        logScope.Log.WriteIndexInsert(index, entity);
                    }

                    logScope.Complete();
                }
            }
            finally
            {
                this.ReleaseWriteLock(table, context);
            }
        }

        #endregion

        #region Delete
       
        public IEnumerable<T> ExecuteDelete<T>(
            IExecutionPlan<IEnumerable<T>> plan, 
            IExecutionContext context) 
            where T : class
        {
            var helper = new ExecutionHelper(this.Database);
            var table = this.Database.Tables.FindTable<T>();
            var cascadedTables = helper.GetCascadedTables(table);
            var allTables = cascadedTables.Concat(new[] { table }).ToArray();

            // Find relations
            // Do not add referred relations!
            RelationGroup allRelations =
                helper.FindRelations(allTables.SelectMany(x => x.Indexes), referred: false);

            this.AcquireWriteLock(table, context);

            var storedEntities = this.Query(plan, table, context);

            this.AcquireWriteLock(cascadedTables, context);
            this.LockRelatedTables(allRelations, context, except: allTables);

            using (AtomicLogScope log = this.StartAtomicLogOperation(context))
            {
                IDeletePrimitive primitive = new DeletePrimitive(this.Database, log);

                primitive.Delete(storedEntities);

                log.Complete();
            }

            return storedEntities.ToArray();
        }

        #endregion

        #region Update

        public IEnumerable<T> ExecuteUpdater<T>(
            IExecutionPlan<IEnumerable<T>> plan,
            IUpdater<T> updater,
            IExecutionContext context)
            where T : class
        {
            var helper = new ExecutionHelper(this.Database);
            var table = this.Database.Tables.FindTable<T>();

            Action<T, T> cloner = context.GetService<IEntityService>().CloneProperties<T>;

            // Determine which indexes are affected by the change
            // If the key of an index containes a changed property, it is affected
            IList<IIndex<T>> affectedIndexes = 
                helper.FindAffectedIndexes(table, updater.Changes);

            // Find relations
            // Add both referring and referred relations!
            RelationGroup relations = helper.FindRelations(affectedIndexes);

            this.AcquireWriteLock(table, context);

            var storedEntities = Query(plan, table, context);

            // Lock related tables (based on found relations)
            this.LockRelatedTables(relations, context, table);

            // Find the entities referring the entities that are about to be updated
            var referringEntities =
                helper.FindReferringEntities(storedEntities, relations.Referring);

            using (AtomicLogScope logScope = this.StartAtomicLogOperation(context))
            {
                // Delete invalid index records (keys are invalid)
                for (int i = 0; i < storedEntities.Count; i++)
                {
                    T storedEntity = storedEntities[i];

                    foreach (IIndex<T> index in affectedIndexes)
                    {
                        index.Delete(storedEntity);
                        logScope.Log.WriteIndexDelete(index, storedEntity);
                    }
                }

                // Modify entity properties
                for (int i = 0; i < storedEntities.Count; i++)
                {
                    T storedEntity = storedEntities[i];

                    if (NMemoryManager.DisableObjectCloning)
                    {
                        T newEntity = updater.Update(storedEntity);

                        if(newEntity != storedEntity)
                        {
                            throw new Exception("Oops! When `NMemoryManager.DisableObjectCloning = true`, the `IUpdater<T>.Update` method needs to return the same entity.");
                        }

                        // Apply contraints on the entity
                        table.Contraints.Apply(storedEntity, context, table);
                    }
                    else
                    {
                        // Create backup
                        T backup = Activator.CreateInstance<T>();
                        cloner(storedEntity, backup);
                        T newEntity = updater.Update(storedEntity);

                        // Apply contraints on the entity
                        table.Contraints.Apply(newEntity, context, table);

                        // Update entity
                        cloner(newEntity, storedEntity);
                        logScope.Log.WriteEntityUpdate(cloner, storedEntity, backup);
                    }   
                }

                // Insert to indexes the entities were removed from
                for (int i = 0; i < storedEntities.Count; i++)
                {
                    T storedEntity = storedEntities[i];

                    foreach (IIndex<T> index in affectedIndexes)
                    {
                        index.Insert(storedEntity);
                        logScope.Log.WriteIndexInsert(index, storedEntity);
                    }
                }

                // Validate the updated entities
                helper.ValidateForeignKeys(relations.Referred, storedEntities);

                // Validate the entities that were referring to the old version of entities
                helper.ValidateForeignKeys(relations.Referring, referringEntities);

                logScope.Complete();
            }

            return storedEntities;
        }

        #endregion

        protected IDatabase Database
        {
            get { return this.database; }
        }

        protected IConcurrencyManager ConcurrencyManager
        {
            get { return this.Database.DatabaseEngine.ConcurrencyManager; }
        }

        #region Locking

        protected void AcquireWriteLock(ITable table, IExecutionContext context)
        {
            this.ConcurrencyManager.AcquireTableWriteLock(table, context.Transaction);
        }

        protected void AcquireWriteLock(ITable[] tables, IExecutionContext context)
        {
            for (int i = 0; i < tables.Length; i++)
            {
                this.AcquireWriteLock(tables[i], context);
            }
        }

        protected void ReleaseWriteLock(ITable table, IExecutionContext context)
        {
            this.ConcurrencyManager.ReleaseTableWriteLock(table, context.Transaction);
        }

        protected void AcquireReadLock(ITable table, IExecutionContext context)
        {
            this.ConcurrencyManager.AcquireTableReadLock(table, context.Transaction);
        }

        protected void ReleaseReadLock(ITable table, IExecutionContext context)
        {
            this.ConcurrencyManager.ReleaseTableReadLock(table, context.Transaction);
        }

        protected void LockRelatedTables(
            ITable[] relatedTables,
            IExecutionContext context)
        {
            for (int i = 0; i < relatedTables.Length; i++)
            {
                this.ConcurrencyManager
                    .AcquireRelatedTableLock(relatedTables[i], context.Transaction);
            }
        }

        private void LockRelatedTables(
            RelationGroup relations,
            IExecutionContext context,
            params ITable[] except)
        {
            var helper = new ExecutionHelper(this.Database);
            ITable[] relatedTables = helper.GetRelatedTables(relations, except).ToArray();

            this.LockRelatedTables(relatedTables, context);
        }

        #endregion

        private List<T> Query<T>(
            IExecutionPlan<IEnumerable<T>> plan,
            ITable<T> table,
            IExecutionContext context)
            where T : class
        {
            ITable[] queryTables = TableLocator.FindAffectedTables(context.Database, plan);

            var query = this.ExecuteQuery(
                plan,
                context,
                queryTables.Except(new[] { table }).ToArray(),
                cloneEntities: false);

            return query.ToEnumerable().ToList();
        }

        private AtomicLogScope StartAtomicLogOperation(IExecutionContext context)
        {
            return new AtomicLogScope(context.Transaction, context.Database);
        }
    }
}
