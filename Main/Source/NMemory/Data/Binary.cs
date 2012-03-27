using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NMemory.Data
{
    public class Binary : IEquatable<Binary>, IEquatable<byte[]>
    {
        private byte[] binary;

        private Binary(byte[] binary)
        {
            if (binary == null)
            {
                throw new ArgumentNullException("binary");
            }

            this.binary = new byte[binary.Length];

            Array.Copy(binary, this.binary, this.binary.Length);
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

        public int Length
        {
            get { return this.binary.Length; }
        }

        public long LongLength
        {
            get { return this.binary.LongLength; }
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
            if (binary == null)
            {
                return null;
            }

            byte[] result = new byte[binary.Length];
            Array.Copy(binary.binary, result, result.Length);

            return result;
        }


        public bool Equals(Binary other)
        {
            if (other == null)
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

        private static unsafe bool AreEqual(byte[] b1, byte[] b2)
        {
            fixed (byte* p1 = b1, p2 = b2)
            {
                byte* x1 = p1, x2 = p2;
                int l = b1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                {
                    if (*((long*)x1) != *((long*)x2))
                    {
                        return false;
                    }
                }
                if ((l & 4) != 0)
                {
                    if (*((int*)x1) != *((int*)x2))
                    {
                        return false;
                    }
                    x1 += 4;
                    x2 += 4;
                }
                if ((l & 2) != 0)
                {
                    if (*((short*)x1) != *((short*)x2))
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
            return this.binary.GetHashCode();
        }


    }
}
