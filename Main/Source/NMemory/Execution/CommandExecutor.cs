// ----------------------------------------------------------------------------------
// <copyright file="CommandExecutor.cs" company="NMemory Team">
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
// ----------------------------------------------------------------------------------

namespace NMemory.Execution
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using NMemory.Common;
    using NMemory.Modularity;
    using NMemory.Tables;
    using NMemory.Transactions;
    using NMemory.Transactions.Logs;
    using NMemory.Utilities;
    using NMemory.Indexes;

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

            EntityPropertyCloner<T> cloner = null;
            if (cloneEntities && this.database.Tables.IsEntityType<T>())
            {
                cloner = EntityPropertyCloner<T>.Instance;
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
                    if (cloner != null)
                    {
                        T resultEntity = Activator.CreateInstance<T>();
                        cloner.Clone(item, resultEntity);

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

            try
            {
                return plan.Execute(context);
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
            ITable<T> table = this.Database.Tables.FindTable<T>();

            table.Contraints.Apply(entity, context);

            // Find referred relations
            // Do not add referring relations!
            RelationGroup relations = this.FindRelations(table.Indexes, referring: false);

            // Acquire locks
            this.AcquireWriteLock(table, context);
            this.LockRelatedTables(table, relations, context);

            try
            {
                // Validate the inserted record
                this.ValidateForeignKeys(relations.Referred, new[] { entity });

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
            ITable<T> table = this.Database.Tables.FindTable<T>();
            ITable[] tables = TableLocator.FindAffectedTables(context.Database, plan);

            // Find relations
            // Do not add referred relations!
            RelationGroup relations = this.FindRelations(table.Indexes, referred: false);

            this.AcquireWriteLock(table, context);

            var query = this.ExecuteQuery(
                plan,
                context,
                tables.Except(new[] { table }).ToArray(),
                cloneEntities: false);
            
            this.LockRelatedTables(table, relations, context);

            var storedEntities = query.ToEnumerable().ToList();

            // Find referring entities
            var referringEntities =
                this.FindReferringEntities(storedEntities, relations.Referring);

            using (AtomicLogScope logScope = this.StartAtomicLogOperation(context))
            {
                // Delete invalid index records
                for (int i = 0; i < storedEntities.Count; i++)
                {
                    T storedEntity = storedEntities[i];

                    foreach (IIndex<T> index in table.Indexes)
                    {
                        index.Delete(storedEntity);
                        logScope.Log.WriteIndexDelete(index, storedEntity);
                    }
                }

                // Validate the entities that are referring to the deleted entities
                this.ValidateForeignKeys(relations.Referring, referringEntities);

                logScope.Complete();
            }

            return storedEntities.ToArray();
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

        private void LockRelatedTables(
            ITable table,
            RelationGroup relations,
            IExecutionContext context)
        {
            List<ITable> relatedTables = this.GetRelatedTables(table, relations).ToList();

            this.LockRelatedTables(relatedTables, context);
        }

        private void LockRelatedTables(
            IEnumerable<ITable> relatedTables,
            IExecutionContext context)
        {
            foreach (ITable table in relatedTables)
            {
                this.ConcurrencyManager
                    .AcquireRelatedTableLock(table, context.Transaction);
            }
        }

        #endregion

        #region Relations

        private IEnumerable<ITable> GetRelatedTables(ITable table, RelationGroup relations)
        {
            return
                relations.Referring.Select(x => x.ForeignTable)
                .Concat(relations.Referred.Select(x => x.PrimaryTable))
                .Distinct()
                .Except(new[] { table }); // This table is already locked
        }

        private RelationGroup FindRelations(
           IEnumerable<IIndex> indexes,
           bool referring = true,
           bool referred = true)
        {
            RelationGroup relations = new RelationGroup();

            foreach (IIndex index in indexes)
            {
                if (referring)
                {
                    foreach (var relation in this.Database.Tables.GetReferringRelations(index))
                    {
                        relations.Referring.Add(relation);
                    }
                }

                if (referred)
                {
                    foreach (var relation in this.Database.Tables.GetReferredRelations(index))
                    {
                        relations.Referred.Add(relation);
                    }
                }
            }

            return relations;
        }

        private Dictionary<IRelation, HashSet<object>> FindReferringEntities<T>(
            IList<T> storedEntities,
            IList<IRelationInternal> relations)
            where T : class
        {
            var result = new Dictionary<IRelation, HashSet<object>>();

            for (int j = 0; j < relations.Count; j++)
            {
                IRelationInternal relation = relations[j];

                HashSet<object> reffering = new HashSet<object>();

                for (int i = 0; i < storedEntities.Count; i++)
                {
                    foreach (object entity in relation.GetReferringEntities(storedEntities[i]))
                    {
                        reffering.Add(entity);
                    }
                }

                result.Add(relation, reffering);
            }

            return result;
        }

        private void ValidateForeignKeys(
           IList<IRelationInternal> relations,
           Dictionary<IRelation, HashSet<object>> referringEntities)
        {
            for (int i = 0; i < relations.Count; i++)
            {
                IRelationInternal relation = relations[i];

                foreach (object entity in referringEntities[relation])
                {
                    relation.ValidateEntity(entity);
                }
            }
        }

        private void ValidateForeignKeys(
            IList<IRelationInternal> relations,
            IEnumerable<object> referringEntities)
        {
            if (relations.Count == 0)
            {
                return;
            }

            foreach (object entity in referringEntities)
            {
                for (int i = 0; i < relations.Count; i++)
                {
                    relations[i].ValidateEntity(entity);
                }
            }
        }

        #endregion

        private AtomicLogScope StartAtomicLogOperation(IExecutionContext context)
        {
            return new AtomicLogScope(context.Transaction, this.Database);
        }
    }
}
