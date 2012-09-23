using System;
using System.Collections.Generic;
using NMemory.Tables;

namespace NMemory.Utilities
{
    public static class TableExtensions
    {
        public static ITable<TEntity> Hint<TEntity>(this ITable<TEntity> table, TableHints hint)
            where TEntity : class
        {
            return table;
        }

        internal static IEnumerable<T> SelectAll<T>(this ITable<T> table)
            where T : class
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }

            return table.PrimaryKeyIndex.SelectAll();
        }
    }
}
