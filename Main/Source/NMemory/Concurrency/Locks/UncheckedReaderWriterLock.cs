using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace NMemory.Execution.Locks
{
    public class UncheckedReaderWriterLock : ILock
    {
        private int readerCount;
        private int writerPending; // flag (1, 0)

        private WaitingToken sentinelToken;
        private LightweightSpinLock innerLock;

        private long id;
        private static long counter;

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
                if (Interlocked.CompareExchange(ref writerPending, 0, 0) == 0)
                {
                    this.innerLock.Enter();

                    if (Interlocked.CompareExchange(ref writerPending, 0, 0) == 0)
                    {
                        Interlocked.Increment(ref readerCount);
                    }

                    this.innerLock.Exit();
                    break;
                }

                LightweightSpinLock.SpinWait(ref retry);
            } 
        }

        public void ExitRead()
        {
            Interlocked.Decrement(ref readerCount);
        }

        public void Upgrade()
        {
            // Indicate that a writer operation entered
            Interlocked.CompareExchange(ref writerPending, 1, 0);

            // Allocate wait token
            WaitingToken wait = this.AllocateWaitingToken();

            Interlocked.Decrement(ref readerCount);

            this.WaitForWrite(wait);
        }


        public void Downgrade()
        {
            Interlocked.Increment(ref this.readerCount);

            DropWaitingToken();
        }


        public void EnterWrite()
        {
            // Indicate that a writer operation entered
            Interlocked.CompareExchange(ref writerPending, 1, 0);

            // Allocate wait token
            WaitingToken wait = this.AllocateWaitingToken();

            this.WaitForWrite(wait);
        }

        public void ExitWrite()
        {
            this.DropWaitingToken();
        }


        private void WaitForWrite(WaitingToken wait)
        {
            int retry = 0;
            while (true)
            {
                // Check if out token is the next and all readers has left
                if (Interlocked.CompareExchange(ref this.sentinelToken.next, wait, wait) == wait &&
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

            while (last.next != null)
            {
                last = last.next;
            }

            // Concat to the end
            last.next = wait;

            this.innerLock.Exit();
            return wait;
        }


        private void DropWaitingToken()
        {
            this.innerLock.Enter();

            // sentinelToken.next = current token (it should not be null)
            WaitingToken next = this.sentinelToken.next.next;
            this.sentinelToken.next = next;

            if (next == null)
            {
                // Indicat that no operation wants to write anymore
                Interlocked.CompareExchange(ref writerPending, 0, 1);
            }

            this.innerLock.Exit();
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
    }
}
