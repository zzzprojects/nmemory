using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Modularity;
using NMemory.Execution;
using NMemory.Common.Visitors;

namespace NMemory.Common
{
    internal static class TableLocator
    {
        public static ITable[] FindAffectedTables(IDatabase database, IExecutionPlan plan)
        {
            EntityTypeSearchVisitor search = new EntityTypeSearchVisitor();
            search.Visit(plan.Plan);

            HashSet<ITable> result = new HashSet<ITable>();

            foreach (Type entityType in search.FoundEntityTypes)
            {
                result.Add(database.Tables.FindTable(entityType));
            }

            return result.ToArray();
        }
    }
}
