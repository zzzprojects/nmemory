// -----------------------------------------------------------------------------------
// <copyright file="ITable.cs" company="NMemory Team">
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
// -----------------------------------------------------------------------------------

namespace NMemory.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NMemory.Indexes;
    using NMemory.Modularity;

    /// <summary>
    ///     Defines an interface for database tables.
    /// </summary>
    public interface ITable : IQueryable
    {
        /// <summary>
        ///     Gets the database that contains the table.
        /// </summary>
        IDatabase Database { get; }

        /// <summary>
        ///     Gets the type of the entities contained in the table.
        /// </summary>
        Type EntityType { get; }

        /// <summary>
        ///     Gets the primary key index of the table.
        /// </summary>
        IIndex PrimaryKeyIndex { get; }
        
        /// <summary>
        ///     Gets the indexes of the table.
        /// </summary>
        IEnumerable<IIndex> Indexes { get; }

        /// <summary>
        ///     Occurs when the indexes of the table are changed.
        /// </summary>
        event EventHandler IndexChanged;

        /// <summary>
        ///     Gets the number of entities contained in the table.
        /// </summary>
        long Count { get; } 
    }
}
