using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;

namespace NMemory.Diagnostics.Messages
{
    public class TableMessage : Message
    {
        public ITable Table { get; set; }

        public override string ToString()
        {
            return string.Format("Table: {0}, Timestamp: {1}", this.Table, this.Timestamp);
        }
    }
}
