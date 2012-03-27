using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Diagnostics.Messages
{
    public class JoinArgumentsOrderSwappedMessage : Message
    {
        public override string ToString()
        {
            return "Arguments order swapped in join operator.";
        }
    }
}
