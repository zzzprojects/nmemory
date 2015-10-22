// ----------------------------------------------------------------------------------
// <copyright file="TransactionLockInventory.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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

namespace NMemory.Concurrency
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using NMemory.Concurrency.Locks;
    using NMemory.Transactions;

    internal class TransactionLockInventory
    {
        private ConcurrentDictionary<Transaction, HashSet<Item>> collection;

        public TransactionLockInventory()
        {
            this.collection = new ConcurrentDictionary<Transaction, HashSet<Item>>();
        }

        public IList<Item> GetAllForRelease(Transaction transaction)
        {
            HashSet<Item> entries = null;
            if (this.collection.TryRemove(transaction, out entries))
            {
                return entries.ToArray();
            }
            else
            {
                return new Item[0];
            }
        }

        // TODO: Reimagine
        public Item GetLockInformation(ILock l, Transaction transaction)
        {
            var set = this.GetLocks(transaction);

            // TODO: Perf
            var lockInfo = set.FirstOrDefault(x => x.Lock.Equals(l));

            if (lockInfo != null)
            {
                return lockInfo;
            }
            else
            {
                var newLockInfo = new Item(l);
                set.Add(newLockInfo);

                return newLockInfo;
            }
        }

        private HashSet<Item> GetLocks(Transaction transaction)
        {
            return this.collection.GetOrAdd(transaction, new HashSet<Item>());
        }

        public class Item
        {
            public Item(ILock l)
            {
                this.Lock = l;
            }

            public ILock Lock { get; private set; }

            public bool IsReadLockHeld { get; set; }

            public bool IsWriteLockHeld { get; set; }

            public bool IsRelatedTable { get; set; }

            public override int GetHashCode()
            {
                return this.Lock.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return this.Lock.Equals(obj);
            }
        }
    }
}
