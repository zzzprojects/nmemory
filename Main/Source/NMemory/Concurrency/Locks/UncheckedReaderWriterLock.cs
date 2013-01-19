// ----------------------------------------------------------------------------------
// <copyright file="UncheckedReaderWriterLock.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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

namespace NMemory.Concurrency.Locks
{
    using System.Threading;

    public class UncheckedReaderWriterLock : ILock
    {
        private static long counter;

        private int readerCount;
        // Flag (bool): 1 vs 0
        private int writerPending;

        private WaitingToken sentinelToken;
        private LightweightSpinLock innerLock;

        private long id;
        
        public UncheckedReaderWriterLock()
        {
            this.sentinelToken = new WaitingToken();
            this.innerLock = new LightweightSpinLock();
            this.id = Interlocked.Increment(ref counter);
            this.readerCount = 0;
            this.writerPending = 0;
        }

        public void EnterRead()
        {
            int retry = 0;
            while (true)
            {
                if (Interlocked.CompareExchange(ref this.writerPending, 0, 0) == 0)
                {
                    this.innerLock.Enter();

                    if (Interlocked.CompareExchange(ref this.writerPending, 0, 0) == 0)
                    {
                        Interlocked.Increment(ref this.readerCount);
                    }

                    this.innerLock.Exit();
                    break;
                }

                LightweightSpinLock.SpinWait(ref retry);
            } 
        }

        public void ExitRead()
        {
            Interlocked.Decrement(ref this.readerCount);
        }

        public void Upgrade()
        {
            // Indicate that a writer operation entered
            Interlocked.CompareExchange(ref this.writerPending, 1, 0);

            // Allocate wait token
            WaitingToken wait = this.AllocateWaitingToken();

            Interlocked.Decrement(ref this.readerCount);

            this.WaitForWrite(wait);
        }

        public void Downgrade()
        {
            Interlocked.Increment(ref this.readerCount);

            this.DropWaitingToken();
        }

        public void EnterWrite()
        {
            // Indicate that a writer operation entered
            Interlocked.CompareExchange(ref this.writerPending, 1, 0);

            // Allocate wait token
            WaitingToken wait = this.AllocateWaitingToken();

            this.WaitForWrite(wait);
        }

        public void ExitWrite()
        {
            this.DropWaitingToken();
        }

        public override int GetHashCode()
        {
            return (int)this.id;
        }

        public override bool Equals(object obj)
        {
            UncheckedReaderWriterLock otherLock = obj as UncheckedReaderWriterLock;

            return otherLock != null && otherLock.id == this.id;
        }

        private void WaitForWrite(WaitingToken wait)
        {
            int retry = 0;
            while (true)
            {
                // Check if out token is the next and all readers has left
                if (Interlocked.CompareExchange(ref this.sentinelToken.Next, wait, wait) == wait &&
                    Interlocked.CompareExchange(ref this.readerCount, 0, 0) == 0)
                {
                    return;
                }

                LightweightSpinLock.SpinWait(ref retry);
            }
        }

        private WaitingToken AllocateWaitingToken()
        {
            WaitingToken wait = new WaitingToken();

            this.innerLock.Enter();

            // Find the last token
            WaitingToken last = this.sentinelToken;

            while (last.Next != null)
            {
                last = last.Next;
            }

            // Concat to the end
            last.Next = wait;

            this.innerLock.Exit();
            return wait;
        }

        private void DropWaitingToken()
        {
            this.innerLock.Enter();

            // sentinelToken.next = current token (it should not be null)
            WaitingToken next = this.sentinelToken.Next.Next;
            this.sentinelToken.Next = next;

            if (next == null)
            {
                // Indicat that no operation wants to write anymore
                Interlocked.CompareExchange(ref this.writerPending, 0, 1);
            }

            this.innerLock.Exit();
        }
    }
}
