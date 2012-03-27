using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;

namespace NMemory.Tables
{
	public static class TableExtensions
	{
        public static ITable<TEntity> Hint<TEntity>(this ITable<TEntity> table, TableHints hint)
            where TEntity : class
        {
            return table;
        }
	}
}
