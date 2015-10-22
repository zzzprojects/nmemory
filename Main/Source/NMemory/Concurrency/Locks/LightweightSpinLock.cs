// ----------------------------------------------------------------------------------
// <copyright file="LightweightSpinLock.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    internal class LightweightSpinLock
    {
        private static readonly int ProcessorCount = Environment.ProcessorCount; 
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
            if (retry < 10 && ProcessorCount > 1)
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