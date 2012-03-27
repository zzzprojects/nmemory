using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;
using NMemory.Linq.Helpers;
using NMemory.Common;

namespace NMemory.Statistics
{
    public class FixedHeightBuckets<T> : IColumnData
           where T : IConvertible
    {
        public List<MutableTuple<T, T>> Buckets { get; set; }
        public long Count { get; private set; }
        public long BucketSize { get; private set; }

        public FixedHeightBuckets(long bucketSize)
        {
            this.Buckets = new List<MutableTuple<T, T>>();
            this.BucketSize = bucketSize;
        }
        public void Analyse<TEntity>(ITable<TEntity> table, Func<TEntity, T> propertySelector)
        {
            Count = 0;
            //int bucket;
            int bucketCounter = 0;
            T value = default(T);

            foreach (TEntity e in table.OrderBy(propertySelector))
            {
                value = propertySelector(e);

                if (bucketCounter == 0)
                {
                    this.Buckets.Add(new MutableTuple<T, T>());
                    this.Buckets.Last().First = value;
                }
                ++Count;
                ++bucketCounter;
                if (bucketCounter == BucketSize)
                {
                    this.Buckets.Last().Second = value;
                    bucketCounter = 0;
                }
            }
            if (this.Buckets.Count != 0)
                this.Buckets.Last().Second = value;
        }

        public long? ValueCount(object value)
        {
            if (value is T == false)
                return null;
            T valueT = (T)value;
            long valueL = valueT.ToInt64(null);
            long count = 0;
            foreach (MutableTuple<T, T> pair in Buckets)
            {
                long first = pair.First.ToInt64(null);
                long second = pair.Second.ToInt64(null);
                if (first <= valueL && second >= valueL)
                    count += this.BucketSize / (second - first + 1);
            }
            return count;
        }

        public long? Values
        {
            get { throw new NotImplementedException(); }
        }
    }
}
