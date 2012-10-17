// ----------------------------------------------------------------------------------
// <copyright file="DatabaseReflectionHelper.cs" company="NMemory Team">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NMemory.Tables;

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
