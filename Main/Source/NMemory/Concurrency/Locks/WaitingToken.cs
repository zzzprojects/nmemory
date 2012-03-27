using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace NMemory.Concurrency.Locks
{
    internal class WaitingToken
    {
        public WaitingToken next;
    }
}
