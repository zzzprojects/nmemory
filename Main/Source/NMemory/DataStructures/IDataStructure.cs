// ----------------------------------------------------------------------------------
// <copyright file="IDataStructure.cs" company="NMemory Team">
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
// ----------------------------------------------------------------------------------

namespace NMemory.DataStructures
{
    using System.Collections.Generic;

    public interface IDataStructure<TKey, TEntity> : IIntervalSearchable<TKey, TEntity>
    {
        IEnumerable<TKey> AllKeys { get; }

        long Count { get; }

        bool SupportsIntervalSearch { get; }

        IEnumerable<TEntity> Select(TKey value);

        IEnumerable<TEntity> SelectAll();

        bool ContainsKey(TKey key);

        void Clear();

        void Insert(TKey key, TEntity entity);

        void Delete(TKey key, TEntity entity);
    }
}
