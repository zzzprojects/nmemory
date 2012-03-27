using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Tables;

namespace NMemory.Diagnostics.Messages
{
    public class Message
    {
        public Message()
        {
            this.Timestamp = DateTime.UtcNow;
        }

        public DateTime Timestamp { get; private set; }

        public override string ToString()
        {
            return string.Format("Timestamp: {0}", this.Timestamp);
        }
    }
}
