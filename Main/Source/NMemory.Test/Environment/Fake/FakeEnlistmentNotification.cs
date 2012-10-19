// -----------------------------------------------------------------------------------
// <copyright file="FakeEnlistmentNotification.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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
// -----------------------------------------------------------------------------------

namespace NMemory.Test.Environment.Fake
{
    using System.Transactions;

    public class FakeEnlistmentNotification : IEnlistmentNotification
    {
        private bool commit;

        public FakeEnlistmentNotification(bool commit)
        {
            this.commit = commit;
        }

        public void Commit(Enlistment enlistment)
        {
            this.WasCommit = true;
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            this.WasRollback = true;
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            this.WasInDoubt = true;
            enlistment.Done();
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            if (commit)
            {
                preparingEnlistment.Prepared();
            }
            else
            {
                preparingEnlistment.ForceRollback();
                // Rollback is not called after 'Rollback' vote
                this.WasRollback = true;
                preparingEnlistment.Done();
            }
        }

        public bool WasCommit { get; private set; }

        public bool WasRollback { get; private set; }

        public bool WasInDoubt { get; private set; }

    }
}
