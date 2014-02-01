// ----------------------------------------------------------------------------------
// <copyright file="ExecutionHelper.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
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
// --------------------------------------------------------------------------------

namespace NMemory.Execution
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NMemory.Indexes;
    using NMemory.Modularity;
    using NMemory.Tables;

    internal class ExecutionHelper
    {
        private readonly IDatabase database;

        public ExecutionHelper(IDatabase database)
        {
            this.database = database;
        }

        #region Relations

        public IEnumerable<ITable> GetRelatedTables(
            RelationGroup relations,
            params ITable[] except)
        {
            return
                relations.Referring.Select(x => x.ForeignTable)
                .Concat(relations.Referred.Select(x => x.PrimaryTable))
                .Distinct()
                .Except(except);
        }

        public RelationGroup FindRelations(
           IEnumerable<IIndex> indexes,
           bool referring = true,
           bool referred = true)
        {
            RelationGroup relations = new RelationGroup();

            foreach (IIndex index in indexes)
            {
                if (referring)
                {
                    foreach (var relation in this.database.Tables.GetReferringRelations(index))
                    {
                        if (!relations.Referring.Contains(relation))
                        {
                            relations.Referring.Add(relation);
                        }
                    }
                }

                if (referred)
                {
                    foreach (var relation in this.database.Tables.GetReferredRelations(index))
                    {
                        if (!relations.Referred.Contains(relation))
                        {
                            relations.Referred.Add(relation);
                        }
                    }
                }
            }

            return relations;
        }

        public Dictionary<IRelation, HashSet<object>> FindReferringEntities<T>(
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

        public void ValidateForeignKeys(
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

        public void ValidateForeignKeys(
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

        public ITable[] GetCascadedTables(ITable table)
        {
            List<ITable> tables = new List<ITable>();

            CollectAllCascadedTables(table, tables);

            return tables
                .Except(new[] { table })
                .ToArray();
        }

        private void CollectAllCascadedTables(ITable currentTable, List<ITable> tables)
        {
            var relations = this.FindRelations(currentTable.Indexes, referred: false)
                .Referring;

            var referringTables = relations
                .Where(x => x.Options.CascadedDeletion)
                .Select(x => x.ForeignTable)
                .ToList();

            foreach (ITable table in referringTables)
            {
                if (!tables.Contains(table))
                {
                    tables.Add(table);
                    CollectAllCascadedTables(currentTable, tables);
                }
            }
        }

        public IList<IIndex<T>> FindAffectedIndexes<T>(ITable<T> table, MemberInfo[] changes)
            where T : class
        {
            IList<IIndex<T>> affectedIndexes = new List<IIndex<T>>();

            foreach (IIndex<T> index in table.Indexes)
            {
                if (index.KeyInfo.EntityKeyMembers.Any(x => changes.Contains(x)))
                {
                    affectedIndexes.Add(index);
                }
            }
            return affectedIndexes;
        }

        #endregion
    }
}
