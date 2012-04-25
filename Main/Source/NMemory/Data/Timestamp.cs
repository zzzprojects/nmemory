using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NMemory.Data
{
    public class Timestamp : IEquatable<Timestamp>
    {
        // 8 byte length binary
        private long value;

        private static long counter;

        public bool Equals(Timestamp other)
        {
            return this.value == other.value;
        }

        public static Timestamp CreateNew()
        {
            Timestamp timestamp = new Timestamp();
            timestamp.value = Interlocked.Increment(ref counter);

            return timestamp;
        }

        public static Timestamp FromBytes(byte[] bytes)
        {
            Timestamp timestamp = new Timestamp();
            timestamp.value = BitConverter.ToInt64(bytes, 0);

            return timestamp;
        }

        public static byte[] GetBytes(Timestamp timestamp)
        {
            return  BitConverter.GetBytes(timestamp.value);
        }

        public static implicit operator Timestamp(byte[] bytes)
        {
            return Timestamp.FromBytes(bytes);
        }

        public static implicit operator byte[](Timestamp timestamp)
        {
            return Timestamp.GetBytes(timestamp);
        }

        public static implicit operator Timestamp(Binary binary)
        {
            return Timestamp.FromBytes(binary);
        }

        public static implicit operator Binary(Timestamp timestamp)
        {
            return Timestamp.GetBytes(timestamp);
        }


    }
}
