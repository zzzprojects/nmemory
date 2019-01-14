# Delete Data

## Introduction

**Table.Delete** deletes an entity from the table. It takes the following arguments by using different overloads.

 - **entity**: An entity that contains the primary key of the entity to be deleted.
 - **transaction**: The transaction within which the delete operation executes.

## Delete Single Entity

To delete a single entity from a table, pass an entity that contains the primary key to be deleted as an argument in the Delete method.

{% include template-example.html %} 
```csharp

MyDatabase myDatabase = new MyDatabase();

myDatabase.Groups.Insert(new Group { Name = "Group 1" });
myDatabase.Groups.Insert(new Group { Name = "Group 2" });

myDatabase.Groups.Delete(new Group() { Id = 1 });

Group group = myDatabase.Groups.FirstOrDefault();

```
[Try it](https://dotnetfiddle.net/uRcI8e)

## Delete All

To delete all records from a table using a loop with Delete().

{% include template-example.html %} 
```csharp

MyDatabase myDatabase = new MyDatabase();

myDatabase.Groups.Insert(new Group { Name = "Group 1" });
myDatabase.Groups.Insert(new Group { Name = "Group 2" });
myDatabase.Groups.Insert(new Group { Name = "Group 3" });

myDatabase.Groups.ToList().ForEach(x => myDatabase.Groups.Delete(x));

Group group = myDatabase.Groups.FirstOrDefault();

```
[Try it](https://dotnetfiddle.net/ZF8PdO)
