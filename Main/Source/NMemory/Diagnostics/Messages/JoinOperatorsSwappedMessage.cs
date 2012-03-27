using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Diagnostics.Messages
{
    public class JoinOperatorsSwappedMessage : Message
    {
        public override string ToString()
        {
            return "Two join operations are swapped.";
        }
    }
}
