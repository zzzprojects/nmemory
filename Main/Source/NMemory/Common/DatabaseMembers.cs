// ----------------------------------------------------------------------------------
// <copyright file="DatabaseMembers.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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

namespace NMemory.Common
{
    using System.Collections.Generic;
    using System.Reflection;
    using NMemory.Execution;
    using NMemory.Modularity;
    using NMemory.Tables;
    using NMemory.Utilities;

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
                    c.GetParameter<object>(string.Empty))
                    
                    .GetGenericMethodDefinition();
            }
        }
    }
}
