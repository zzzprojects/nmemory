using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Execution.Locks
{
    public class DefaultLockFactory : ILockFactory
    {
        public ILock CreateLock()
        {
            return new UncheckedReaderWriterLock();
        }
    }
}
