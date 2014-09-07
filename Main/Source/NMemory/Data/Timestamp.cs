// ----------------------------------------------------------------------------------
// <copyright file="Timestamp.cs" company="NMemory Team">
//     Copyright (C) 2012-2014 NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class Timestamp : IEquatable<Timestamp>, IComparable<Timestamp>, IComparable
    {
        private static long counter;

        // 8 byte length binary
        private long value;

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
            return BitConverter.GetBytes(timestamp.value);
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

        public bool Equals(Timestamp other)
        {
            return this.value == other.value;
        }

        public int CompareTo(Timestamp other)
        {
            return this.value.CompareTo(other.value);
        }

        public int CompareTo(object obj)
        {
            var ts = obj as Timestamp;
            if (ts == null)
            {
                return -1;
            }
            return this.CompareTo(ts);
        }
    }
}
