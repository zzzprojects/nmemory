using System.Collections.Generic;
using System.Reflection;
using NMemory.Execution;
using NMemory.Modularity;
using NMemory.Tables;
using NMemory.Utilities;

namespace NMemory.Common
{
    internal static class DatabaseMembers
    {
        public static PropertyInfo Database_Tables
        {
            get
            {
                return ReflectionHelper.GetPropertyInfo<IDatabase, TableCollection>(d => d.Tables);
            }
        }

        public static MethodInfo TableCollection_FindTable
        {
            get
            {
                return ReflectionHelper.GetMethodInfo<TableCollection>(t => 
                    t.FindTable(null));
            }
        }

        public static MethodInfo TableCollectionExtensions_FindTable
        {
            get
            {
                return ReflectionHelper.GetStaticMethodInfo(() => 
                    TableCollectionExtensions.FindTable<object>(null))

                    .GetGenericMethodDefinition();
            }
        }

        public static MethodInfo TableExtensions_SelectAll
        {
            get
            {
                return ReflectionHelper.GetStaticMethodInfo<IEnumerable<object>>(() =>
                    TableExtensions.SelectAll<object>(null)).GetGenericMethodDefinition();
            }
        }

        public static PropertyInfo ExecutionContext_Database
        {
            get
            {
                return ReflectionHelper.GetPropertyInfo<IExecutionContext, IDatabase>(c => c.Database);
            }
        }

        public static MethodInfo ExecutionContext_GetParameter
        {
            get
            {
                return ReflectionHelper.GetMethodInfo<IExecutionContext>(c => 
                    c.GetParameter<object>(""))
                    
                    .GetGenericMethodDefinition();
            }
        }

        

        


    }
}
