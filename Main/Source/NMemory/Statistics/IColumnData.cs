using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Statistics
{
    public interface IColumnData
    {
        long? Values { get; }
        long Count { get; }
        long? ValueCount(object value);
    }
}
