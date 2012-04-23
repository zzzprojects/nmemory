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

        public static Timestamp FromBinary(byte[] binary)
        {
            Timestamp timestamp = new Timestamp();
            timestamp.value = BitConverter.ToInt64(binary, 0);

            return timestamp;
        }

        public bool Equals(Timestamp other)
        {
            return this.value == other.value;
        }

        public static byte[] GetBytes(Timestamp timestamp)
        {
            return  BitConverter.GetBytes(timestamp.value);
        }
    }
}
