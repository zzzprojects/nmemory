using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NMemory.Statistics
{
    public interface IStatistics
    {
        long Count { get; }
        long? Values(MemberInfo member);
        long? ValueCount(MemberInfo member, object value);
    }
}
