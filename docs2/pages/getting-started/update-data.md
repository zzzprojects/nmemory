# Update Data

## Introduction

**Table.Update** updates the properties of the specified entity contained in the table. It takes the following arguments by using different overloads.

 - **entity**: An entity that contains the primary key of the entity to be updated and the new property values.
 - **transaction**: The transaction within which the update operation executes.

## Update Single Entity

To update properties of an entity in a table, pass an entity that contains the primary key to be updated as an argument in the Update method.

{% include template-example.html %} 
```csharp

MyDatabase database = new MyDatabase();

Member member = new Member { Id = "JD1967", Name = "Joh Doe" };
database.Members.Insert(member);

member.Name = "John Doe";

database.Members.Update(member);

Member updated = database.Members.FirstOrDefault();

```

## Update Multiple Entities

To update multiple records in a table.

{% include template-example.html %} 
```csharp

MyDatabase database = new MyDatabase();

database.Groups.Insert(new Group { Name = "Group 1" });
database.Groups.Insert(new Group { Name = "Group 2" });
database.Groups.Insert(new Group { Name = "Group 3" });

database.Groups.Update(x => new Group { Name = x.Name + " (deleted)" });

var newEntities = database.Groups.ToList();

```



