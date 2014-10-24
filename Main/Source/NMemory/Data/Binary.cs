// ----------------------------------------------------------------------------------
// <copyright file="Binary.cs" company="NMemory Team">
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

    public class Binary : 
        IEquatable<Binary>, IEquatable<byte[]>, 
        IComparable<Binary>, IComparable<byte[]>
    {
        private readonly byte[] binary;

        private Binary(byte[] binary)
        {
            if (binary == null)
            {
                throw new ArgumentNullException("binary");
            }

            this.binary = new byte[binary.Length];

            Array.Copy(binary, this.binary, this.binary.Length);
        }

        public int Length
        {
            get { return this.binary.Length; }
        }

        public long LongLength
        {
            get { return this.binary.LongLength; }
        }

        public byte this[int index]
        {
            get
            {
                return this.binary[index];
            }

            set
            {
                this.binary[index] = value;
            }
        }

        public static implicit operator Binary(byte[] binary)
        {
            if (binary == null)
            {
                return null;
            }

            return new Binary(binary);
        }

        public static implicit operator byte[](Binary binary)
        {
            if (((object)binary) == null)
            {
                return null;
            }

            byte[] result = new byte[binary.Length];
            Array.Copy(binary.binary, result, result.Length);

            return result;
        }

        public static bool operator ==(Binary left, Binary right)
        {
            if (((object)left) == null || ((object)right) == null)
            {
                return ((object)left) == null && ((object)right) == null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Binary left, Binary right)
        {
            return !(left == right);
        }

        public static bool operator >(Binary left, Binary right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(Binary left, Binary right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(Binary left, Binary right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(Binary left, Binary right)
        {
            return left.CompareTo(right) <= 0;
        }

        public bool Equals(Binary other)
        {
            if (((object)other) == null)
            {
                return false;
            }

            if (other.binary.Length != this.binary.Length)
            {
                return false;
            }

            return AreEqual(this.binary, other.binary);
        }

        public bool Equals(byte[] other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.Length != this.binary.Length)
            {
                return false;
            }

            return AreEqual(this.binary, other);
        }

        public override bool Equals(object obj)
        {
            if (obj is Binary)
            {
                return this.Equals((Binary)obj);
            }

            if (obj is byte[])
            {
                return this.Equals((byte[])obj);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return GetHashCode(this.binary);
        }

        public int CompareTo(Binary other)
        {
            return CompareTo((byte[])other);
        }

        public int CompareTo(byte[] other)
        {
            if (other == null)
            {
                return 1;
            }

            int lengthCompare = this.binary.Length.CompareTo(other.Length);

            if (lengthCompare != 0)
            {
                return lengthCompare;
            }

            return Compare(this.binary, other);
        }

        private static unsafe int Compare(byte[] b1, byte[] b2)
        {
            int res = 0;

            fixed (byte* p1 = b1, p2 = b2)
            {
                byte* x1 = p1, x2 = p2;
                int l = b1.Length;

                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                {
                    res = (*((ulong*)x1)).CompareTo(*((ulong*)x2));

                    if (res != 0)
                    {
                        return res;
                    }
                }

                if ((l & 4) != 0)
                {
                    res = (*((uint*)x1)).CompareTo(*((uint*)x2));

                    if (res != 0)
                    {
                        return res;
                    }

                    x1 += 4;
                    x2 += 4;
                }

                if ((l & 2) != 0)
                {
                    res = (*((ushort*)x1)).CompareTo(*((ushort*)x2));

                    if (res != 0)
                    {
                        return res;
                    }

                    x1 += 2;
                    x2 += 2;
                }

                if ((l & 1) != 0)
                {
                    res = (*((byte*)x1)).CompareTo(*((byte*)x2));
                }

                return res;
            }
        }

        private static unsafe bool AreEqual(byte[] b1, byte[] b2)
        {
            fixed (byte* p1 = b1, p2 = b2)
            {
                byte* x1 = p1, x2 = p2;
                int l = b1.Length;

                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                {
                    if (*((ulong*)x1) != *((ulong*)x2))
                    {
                        return false;
                    }
                }

                if ((l & 4) != 0)
                {
                    if (*((uint*)x1) != *((uint*)x2))
                    {
                        return false;
                    }

                    x1 += 4;
                    x2 += 4;
                }

                if ((l & 2) != 0)
                {
                    if (*((ushort*)x1) != *((ushort*)x2))
                    {
                        return false;
                    }

                    x1 += 2;
                    x2 += 2;
                }

                if ((l & 1) != 0)
                {
                    if (*((byte*)x1) != *((byte*)x2))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static unsafe int GetHashCode(byte[] b)
        {
            int result = 0;

            fixed (byte* p = b)
            {
                int l = b.Length;
                byte* x = p;

                for (int i = 0; i < l / 4; i++, x += 4)
                {
                    result ^= (*((int*)x));
                }

                if ((l & 2) != 0)
                {
                    result ^= (*((short*)x));

                    x += 2;
                }

                if ((l & 1) != 0)
                {
                    result ^= (*((byte*)x));
                }
            }

            return result;
        }
    }
}
