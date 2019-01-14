# Stored Procedures

## Introduction

A stored procedure is nothing more than prepared SQL code that you save so you can reuse the code over and over again. The commands in a procedure are executed as a single batch of code. 
  
 - Reuse of code. 
 - Easier in maintenance
 - Improved performance

**NMemory** provides a class **StoredProcedureCollection** which represents a collection of the stored procedure in a database. **StoredProcedureCollection.Create** method creates a stored procedure and it takes an IQueryable object as an argument.

{% include template-example.html %} 
```csharp

MyDatabase database = new MyDatabase();

IQueryable<Group> query = 
                database.Groups.Where(g =>
                    g.Id > new Parameter<int>("param1") + new Parameter<long?>("param2"));

var procedure = database.StoredProcedures.Create(query);
```
[Try it](https://dotnetfiddle.net/3OGtT3)

A shared, stored procedure creates the query every time the Execute method is called.

{% include template-example.html %} 
```csharp
MyDatabase database = new MyDatabase();

database.Groups.Insert(new Group { Name = "Group 1" });
database.Groups.Insert(new Group { Name = "Group 2" });
database.Groups.Insert(new Group { Name = "Group 3" });

var procedure = new SharedStoredProcedure<MyDatabase, Group>(
    d => d.Groups.Where(g => g.Id > 1));

var result = procedure.Execute(database, null).ToList();

```
[Try it](https://dotnetfiddle.net/EEWXoI)

