using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NMemory.Concurrency.Locks
{
    internal struct LightweightSpinLock
    {
        private static readonly int processorCount = Environment.ProcessorCount; 
        private int held;

        public void Enter()
        {
            if (Interlocked.CompareExchange(ref this.held, 1, 0) == 0)
            {
                return;
            }

            int retry = 0;
            while (true)
            {
                LightweightSpinLock.SpinWait(ref retry);

                if ((this.held == 0) && (Interlocked.CompareExchange(ref this.held, 1, 0) == 0))
                {
                    return;
                }
            }
        }

        public void Exit()
        {
            this.held = 0;
        }

        internal static void SpinWait(ref int retry)
        {
            if (retry < 10 && processorCount > 1)
            {
                Thread.SpinWait(20 * (retry + 1));
            }
            else if (retry < 15)
            {
                Thread.Sleep(0);
            }
            else
            {
                Thread.Sleep(1);
            }

            retry++;
        }


    }
}
