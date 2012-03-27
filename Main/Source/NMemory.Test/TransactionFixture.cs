using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMemory.Test.Data;
using System.Transactions;
using System.Threading;
using NMemory.Test.Fake;

namespace NMemory.Test
{
    [TestClass]
    public class TransactionFixture
    {
        /// <summary>
        /// Execute and commit data operation within a transaction scope
        /// </summary>
        [TestMethod]
        public void TransactionScope()
        {
            TestDatabase database = new TestDatabase();

            using (TransactionScope tran = new TransactionScope())
            {
                database.Members.Insert(new Member { Id = "A" });
                tran.Complete();
            }

            Assert.AreEqual(database.Members.Count, 1);
            Assert.IsTrue(database.Members.Any(m => m.Id == "A"));
        }

        /// <summary>
        /// Execute and rollback data operation within a transaction scope
        /// </summary>
        [TestMethod]
        public void LocalTransactionRollback()
        {
            TestDatabase database = new TestDatabase();
            database.AddGroupNameIndex();

            bool failed = false;

            database.Groups.Insert(new Group { Name = "A" });
            try
            {
                database.Groups.Insert(new Group { Name = "A" });
            }
            catch
            {
                failed = true;
            }

            Assert.IsTrue(failed);
            Assert.AreEqual(database.Groups.Count, 1);

        }

        /// <summary>
        /// The transaction has to be rollbacked
        /// </summary>
        [TestMethod]
        public void TransactionRollback()
        {
            TestDatabase database = new TestDatabase();
            // 'false' indicates that it will vote 'Rollback'
            FakeEnlistmentNotification enlistment = new FakeEnlistmentNotification(false);

            try
            {
                using (TransactionScope tran = new TransactionScope())
                {
                    Transaction.Current.EnlistVolatile(enlistment, EnlistmentOptions.EnlistDuringPrepareRequired);

                    database.Members.Insert(new Member { Id = "A" });

                    tran.Complete();
                }
            }
            catch { }

            Assert.AreEqual(database.Members.Count, 0);
            Assert.IsFalse(database.Members.Any(m => m.Id == "A"));
            Assert.IsTrue(enlistment.WasRollback);
            Assert.IsFalse(enlistment.WasCommit);

        }


        /// <summary>
        /// The transaction has to be commited
        /// </summary>
        [TestMethod]
        public void TransactionCommit()
        {
            TestDatabase database = new TestDatabase();
            // 'true' indicates that it will vote 'Commit'
            FakeEnlistmentNotification enlistment = new FakeEnlistmentNotification(true);

            try
            {
                using (TransactionScope tran = new TransactionScope())
                {
                    Transaction.Current.EnlistVolatile(enlistment, EnlistmentOptions.EnlistDuringPrepareRequired);

                    database.Members.Insert(new Member { Id = "A" });

                    tran.Complete();
                }
            }
            catch { }

            Assert.AreEqual(database.Members.Count, 1);
            Assert.IsTrue(database.Members.Any(m => m.Id == "A"));
            Assert.IsFalse(enlistment.WasRollback);
            Assert.IsTrue(enlistment.WasCommit);

        }

        /// <summary>
        /// Enlist the transaction to a manually created, external Transaction object that propagates 'Commit'
        /// </summary>
        [TestMethod]
        public void ExternalTransactionCommit()
        {
            TestDatabase database = new TestDatabase();

            CommittableTransaction external = new CommittableTransaction();
            NMemory.Transactions.Transaction.EnlistOnExternal(external);

            database.Members.Insert(new Member { Id = "A" });
            external.Commit();

            Assert.AreEqual(database.Members.Count, 1);
            Assert.IsTrue(database.Members.Any(m => m.Id == "A"));
        }

        /// <summary>
        /// Transaction is enlisted on a non-ambient Transaction object that propagates 'Rollback'
        /// </summary>
        [TestMethod]
        public void ExternalTransactionRollback()
        {
            TestDatabase database = new TestDatabase();

            CommittableTransaction external = new CommittableTransaction();
            NMemory.Transactions.Transaction.EnlistOnExternal(external);

            database.Members.Insert(new Member { Id = "A" });
            external.Rollback();

            Assert.AreEqual(database.Members.Count, 0);
            Assert.IsFalse(database.Members.Any(m => m.Id == "A"));
        }

        /// <summary>
        /// Transaction is enlisted on a non-ambient Transaction object that times out
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TransactionAbortedException))]
        public void ExternalTransactionTimeout()
        {
            TestDatabase database = new TestDatabase();

            CommittableTransaction external = new CommittableTransaction(TimeSpan.FromMilliseconds(50));
            NMemory.Transactions.Transaction.EnlistOnExternal(external);

            database.Groups.Insert(new Group { Name = "Group1" });
            Thread.Sleep(1000);
            database.Groups.Insert(new Group { Name = "Group2" });
        }

        /// <summary>
        /// Nested transactions
        /// </summary>
        [TestMethod]
        public void NestedTransaction()
        {
            FakeEnlistmentNotification en1 = new FakeEnlistmentNotification(true);
            FakeEnlistmentNotification en2 = new FakeEnlistmentNotification(true);

            using (TransactionScope tran1 = new TransactionScope())
            {
                Transaction.Current.EnlistVolatile(en1, EnlistmentOptions.EnlistDuringPrepareRequired);

                using (TransactionScope tran2 = new TransactionScope())
                {
                    Transaction.Current.EnlistVolatile(en2, EnlistmentOptions.EnlistDuringPrepareRequired);
                    tran2.Complete();
                }

                var current = Transaction.Current;
                tran1.Complete();
            }
        }


    }
}
