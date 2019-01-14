# Query Data

## Introduction

To retrieve using LINQ query, the first step is to specify the data source and a variable must be declared before it can be used.

{% include template-example.html %} 
```csharp

MyDatabase db = new MyDatabase();

var query =
    from p in db.Members
    join g in db.Groups on p.GroupId equals g.Id
    select new { Name = p.Name, Group = g.Name };
    
query.ToList()

```
[Try it](https://dotnetfiddle.net/ru3YVb)

You can also use LINQ to Entities, e.g., to enumerate a table.

{% include template-example.html %} 
```csharp

MyDatabase db = new MyDatabase();

var groups = db.Groups.ToList();
var members = db.Members.ToList();

```
[Try it](https://dotnetfiddle.net/7v52xn)



