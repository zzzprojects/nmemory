using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using NMemory.Transactions;
using NMemory.Concurrency.Locks;

namespace NMemory.Concurrency
{
    public class TransactionLockInventory
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
            var set = GetLocks(transaction);

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
