# Insert Data

## Introduction

**Table.Insert** inserts the specified entity into the table. It takes the following arguments by using different overloads.

 - **entity**: An entity that contains the property values of the new entity.
 - **transaction**: The transaction within which the insert operation executes.

{% include template-example.html %} 
```csharp

MyDatabase db = new MyDatabase();

db.Groups.Insert(new Group { 
    Id = 1, 
    Name = "Alpha Group" });

db.Groups.Insert(new Group { 
    Id = 2, 
    Name = "Beta Group" });

db.Members.Insert(new Member { 
    Id = "JD", 
    Name = "John Doe", 
    GroupId = 1});

```
[Try it](https://dotnetfiddle.net/Ch0plY)

