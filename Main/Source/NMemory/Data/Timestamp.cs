using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NMemory.Data
{
    public struct Timestamp : IEquatable<Timestamp>
    {
        // 8 byte length binary
        private long value;

        private static long counter;

        public static Timestamp CreateNew()
        {
            Timestamp timestamp = new Timestamp();
            timestamp.value = Interlocked.Increment(ref counter);

            return timestamp;
        }

        public bool Equals(Timestamp other)
        {
            return this.value == other.value;
        }
    }
}
