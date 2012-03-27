using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Concurrency
{
    /// <summary>
    /// Specifies the stragegy of deadlock management
    /// </summary>
    public enum DeadlockManagementStrategies
    {
        /// <summary>
        /// Deadlock detection
        /// </summary>
        DeadlockDetection,

        /// <summary>
        /// Deadlock prevention
        /// </summary>
        DeadlockPrevention
    }
}
