# Update Data

## Introduction

**Table.Update** updates the properties of the specified entity contained in the table. It takes the following arguments by using different overloads.

 - **entity**: An entity that contains the primary key of the entity to be updated and the new property values.
 - **transaction**: The transaction within which the update operation executes.

## Update Single Entity

To update properties of an entity in a table, pass an entity that contains the primary key to be updated as an argument in the Update method.

{% include template-example.html %} 
```csharp

MyDatabase myDatabase = new MyDatabase();

Member member = new Member { Id = "JD1967", Name = "Joh Doe" };
myDatabase.Members.Insert(member);

member.Name = "John Doe";

myDatabase.Members.Update(member);

Member updated = database.Members.FirstOrDefault();

```
[Try it](https://dotnetfiddle.net/PVU58y)

## Update Multiple Entities

To update multiple records in a table.

{% include template-example.html %} 
```csharp

 MyDatabase myDatabase = new MyDatabase();

 myDatabase.Groups.Insert(new Group { Id = 1, Name = "Group 1" });
 myDatabase.Groups.Insert(new Group { Id = 2,Name = "Group 2" });
 myDatabase.Groups.Insert(new Group { Id = 3,Name = "Group 3" });

 var groups = myDatabase.Groups.ToList();
 groups.ForEach(x => x.Name += " (deleted)" );
 groups.ForEach(x => myDatabase.Groups.Update(x));

var newEntities = myDatabase.Groups.ToList();

```
[Try it](https://dotnetfiddle.net/ImQTZ5)



