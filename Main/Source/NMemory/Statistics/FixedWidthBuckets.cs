using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;

namespace NMemory.Statistics
{
    public class FixedWidthBuckets<T> : IColumnData
    {
        public Dictionary<int, long> Frequencies { get; set; }
        public Func<T, int> BucketSelector { get; set; }
        public long Count { get; private set; }
        public long BucketSize { get; private set; }

        public FixedWidthBuckets(Func<T, int> bucketSelector, long bucketSize)
        {
            this.Frequencies = new Dictionary<int, long>();
            this.BucketSelector = bucketSelector;
            this.BucketSize = bucketSize;
        }

        public void Analyse<TEntity>(ITable<TEntity> table, Func<TEntity, T> propertySelector)
        {
            Count = 0;
            int bucket;
            foreach (TEntity e in table)
            {
                ++Count;
                bucket = this.BucketSelector(propertySelector(e));
                long value;

                if (this.Frequencies.TryGetValue(bucket, out value) == true)
                {
                    this.Frequencies[bucket] = value + 1;
                }
                else
                {
                    this.Frequencies[bucket] = 1;
                }
            }
        }

        public long? ValueCount(object value)
        {
            if (value is T == false)
            {
                return null;
            }

            T valueT = (T)value;
            long freq;

            if (Frequencies.TryGetValue(BucketSelector(valueT), out freq) == true)
            {
                return freq / BucketSize;
            }
            else
            {
                return null;
            }
        }

        public long? Values
        {
            get { return this.Frequencies.Count; }
        }
    }
}
