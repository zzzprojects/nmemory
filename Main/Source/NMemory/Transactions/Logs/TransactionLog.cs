using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NMemory.Indexes;
using System.Threading;
using NMemory.Tables;

namespace NMemory.Transactions.Logs
{
    public class TransactionLog : ITransactionLog
    {
        private List<ITransactionLogItem> logItems;

        public TransactionLog()
        {
            this.logItems = new List<ITransactionLogItem>();
        }

        public int CurrentPosition
        {
            get { return this.logItems.Count; }
        }

        public void Rollback()
        {
            this.RollbackTo(0);
        }

        public void RollbackTo(int position)
        {
            for (int i = this.logItems.Count - 1; i >= position; i--)
            {
                this.logItems[i].Undo();
                this.logItems.RemoveAt(i);
            }
        }

        public void Write(ITransactionLogItem item)
        {
            this.logItems.Add(item);
        }

        public void Release()
        {
            this.logItems.Clear();
        }
    }
}
