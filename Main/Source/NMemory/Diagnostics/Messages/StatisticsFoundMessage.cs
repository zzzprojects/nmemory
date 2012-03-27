using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NMemory.Statistics;

namespace NMemory.Diagnostics.Messages
{
    public class StatisticsFoundMessage : TableMessage
    {
        public IStatistics Statistics { get; set; }
        public MemberInfo Member { get; set; }
        public object Value { get; set; }
        public long Count { get; set; }

        public override string ToString()
        {
            return string.Format("*** statistics found on {0} with value {1}. Estimated count: {2}", this.Member, this.Value, this.Count);
        }
    }
}
