using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Concurrency.Locks
{
    /// <summary>
    /// Represents a factory that is able to instantiate <c>ILock</c> objects
    /// </summary>
    public interface ILockFactory
    {
        ILock CreateLock();
    }
}
