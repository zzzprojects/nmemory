# Transaction

## Introduction

**Transaction** is a unit of work that is performed against a database. 

 - If a transaction is successful, all the data modifications made during the transaction are committed and become a permanent part of the database.
 - If a transaction encounters errors and must be canceled or rolled back, then all the data modifications are erased.

## Transaction Scope

### Commit Data

Execute and commit data operation within a transaction scope.

{% include template-example.html %} 
```csharp
MyDatabase database = new MyDatabase();

using (TransactionScope tran = new TransactionScope())
{
    database.Members.Insert(new Member { Id = "A" });
    tran.Complete();
}
```

### Rollback Data

Execute and rollback data operation within a transaction scope.

{% include template-example.html %} 
```csharp

MyDatabase database = new MyDatabase();
// 'false' indicates that it will vote 'Rollback'
FakeEnlistmentNotification enlistment = new FakeEnlistmentNotification(false);

try
{
    using (TransactionScope tran = new TransactionScope())
    {
        Transaction.Current.EnlistVolatile(enlistment, 
                                                EnlistmentOptions.EnlistDuringPrepareRequired);

        database.Members.Insert(new Member { Id = "A" });

        tran.Complete();
    }
}
catch { }
```

## Fake Enlistment Notification

The transaction has to be committed, and the `true` value in the **FakeEnlistmentNotification** constructor indicates that it will vote `Commit`.

{% include template-example.html %} 
```csharp
MyDatabase database = new MyDatabase();
// 'true' indicates that it will vote 'Commit'
FakeEnlistmentNotification enlistment = new FakeEnlistmentNotification(true);

try
{
    using (TransactionScope tran = new TransactionScope())
    {
        Transaction.Current.EnlistVolatile(enlistment, 
                                            EnlistmentOptions.EnlistDuringPrepareRequired);

        database.Members.Insert(new Member { Id = "A" });

        tran.Complete();
    }
}
catch { }
```

## External Transaction

Enlist the transaction to a manually created external Transaction object that propagates `Commit`.

{% include template-example.html %} 
```csharp
MyDatabase database = new MyDatabase();

CommittableTransaction external = new CommittableTransaction();
NMemory.Transactions.Transaction transaction = NMemory.Transactions.Transaction.Create(external);

database.Members.Insert(new Member { Id = "A" }, transaction);
external.Commit();

```
