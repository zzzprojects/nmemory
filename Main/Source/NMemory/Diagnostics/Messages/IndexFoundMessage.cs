using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;

namespace NMemory.Diagnostics.Messages
{
    public class IndexFoundMessage : TableMessage
    {
        public IIndex Index { get; set; }

        public override string ToString()
        {
            return string.Format("*** index found: {0}", this.Index);
        }
    }
}
