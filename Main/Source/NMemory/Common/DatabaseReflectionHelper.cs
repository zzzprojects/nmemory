using System;
using System.Collections.Generic;
using System.Linq;
using NMemory.Tables;

namespace NMemory.Common
{
    internal static class DatabaseReflectionHelper
    {
        public static bool IsTable(Type table)
        {
            if (table == null)
            {
                return false;
            }

            return typeof(ITable).IsAssignableFrom(table);
        }

        public static Type GetTableEntityType(Type table)
        {
            if (!IsTable(table))
            {
                return null;
            }

            List<Type> possibleInterfaces = table.GetInterfaces().ToList();
            
            if (table.IsInterface)
	        {
		        possibleInterfaces.Add(table);
	        }

            Type tableInterface =
                possibleInterfaces.SingleOrDefault(i => 
                    i.IsInterface &&
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(ITable<>));

            return tableInterface.GetGenericArguments()[0];
        }
    }
}
