using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace NMemory.Test.Fake
{
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
