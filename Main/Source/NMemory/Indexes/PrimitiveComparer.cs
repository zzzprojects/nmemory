using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Indexes
{
    public static class PrimitiveKeyComparer
    {
        public static int Compare<T>(T x, T y, SortOrder sortOder)
        {
            // Examination of null might not need
            if (x == null && y != null)
            {
                return EvaluateResult(-1, sortOder);
            }

            if (x != null && y == null)
            {
                return EvaluateResult(1, sortOder);
            }

            if (x == null && y == null)
            {
                return 0;
            }

            int result = Comparer<T>.Default.Compare(x, y);

            return EvaluateResult(result, sortOder);
        }

        private static int EvaluateResult(int result, SortOrder sortOrder)
        {
            if (sortOrder == SortOrder.Ascending)
            {
                return result;
            }
            else
            {
                return -result;
            }
        }
    }
}
