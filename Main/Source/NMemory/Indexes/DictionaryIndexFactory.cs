// ----------------------------------------------------------------------------------
// <copyright file="DictionaryIndexFactory.cs" company="NMemory Team">
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

namespace NMemory.Indexes
{
    using NMemory.DataStructures;
    using NMemory.Tables;

    public class DictionaryIndexFactory : IndexFactoryBase
    {
        public DictionaryIndexFactory(IKeyInfoFactory keyInfoFactory)
            : base(keyInfoFactory)
        {
        }

        public DictionaryIndexFactory()
        {
        }

        public override IIndex<TEntity, TKey> CreateIndex<TEntity, TKey>(ITable<TEntity> table, IKeyInfo<TEntity, TKey> keyInfo)
        {
            Hashtable<TKey, TEntity> hashTable = new Hashtable<TKey, TEntity>();
            Index<TEntity, TKey> index = new Index<TEntity, TKey>(table, keyInfo, hashTable);

            return index;
        }

        public override IUniqueIndex<TEntity, TUniqueKey> CreateUniqueIndex<TEntity, TUniqueKey>(ITable<TEntity> table, IKeyInfo<TEntity, TUniqueKey> keyInfo)
        {
            UniqueHashtable<TUniqueKey, TEntity> hashTable = new UniqueHashtable<TUniqueKey, TEntity>();
            UniqueIndex<TEntity, TUniqueKey> index = new UniqueIndex<TEntity, TUniqueKey>(table, keyInfo, hashTable);

            return index;
        }
    }
}
