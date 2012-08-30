using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Indexes
{
    internal class PrimitiveKeyComparer<T> : IComparer<T>
    {
        private SortOrder sortOrder;

        public PrimitiveKeyComparer(SortOrder sortOrder)
        {
            this.sortOrder = sortOrder;
        }

        public int Compare(T x, T y)
        {
            return PrimitiveKeyComparer.Compare<T>(x, y, this.sortOrder);
        }
    }
}
