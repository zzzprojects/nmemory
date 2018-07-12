// ----------------------------------------------------------------------------------
// <copyright file="DeletePrimitive.cs" company="NMemory Team">
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
// --------------------------------------------------------------------------------

namespace NMemory.Execution.Primitives
{
    using System.Collections.Generic;
    using System.Linq;
    using NMemory.Indexes;
    using NMemory.Linq;
    using NMemory.Modularity;
    using NMemory.Tables;
    using NMemory.Transactions.Logs;
    using NMemory.Utilities;

    internal class DeletePrimitive : IDeletePrimitive
    {
        private readonly IDatabase database;
        private readonly AtomicLogScope log;

        public DeletePrimitive(IDatabase database, AtomicLogScope log)
        {
            this.database = database;
            this.log = log;
        }

        public void Delete<T>(IList<T> storedEntities) where T : class
        {
            ExecutionHelper helper = new ExecutionHelper(this.database);
            ITable<T> table = this.database.Tables.FindTable<T>();

            // Find relations
            // Do not add referred relations!
            RelationGroup relations =
                helper.FindRelations(table.Indexes, referred: false);

            // Find referring entities
            var referringEntities =
                helper.FindReferringEntities<T>(storedEntities, relations.Referring);

            // Delete invalid index records
            for (int i = 0; i < storedEntities.Count; i++)
            {
                T storedEntity = storedEntities[i];

                foreach (IIndex<T> index in table.Indexes)
                {
                    index.Delete(storedEntity);
                    log.Log.WriteIndexDelete(index, storedEntity);
                }
            }

            var cascadedRelations = relations.Referring.Where(x => x.Options.CascadedDeletion);

            foreach (IRelationInternal index in cascadedRelations)
            {
                var entities = referringEntities[index];

                if (entities.Count == 0)
                {
                    continue;
                }

                index.CascadedDelete(entities, this);

                // At this point these entities should have been removed from the database
                // In order to avoid foreign key validation, clear the collection
                //
                // TODO: It might be better to do foreign key validation from the
                // other direction: check if anything refers storedEntities
                entities.Clear();
            }

            // Validate the entities that are referring to the deleted entities
            helper.ValidateForeignKeys(relations.Referring, referringEntities);
        }
    }
}
