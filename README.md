## Library Powered By

This library is powered by [Entity Framework Extensions](https://entityframework-extensions.net/?z=github&y=entityframework-plus)

<a href="https://entityframework-extensions.net/?z=github&y=nmemory">
<kbd>
<img src="https://zzzprojects.github.io/images/logo/entityframework-extensions-pub.jpg" alt="Entity Framework Extensions" />
</kbd>
</a>

# What's NMemory?
NMemory is a lightweight non-persistent in-memory relational database engine that is purely written in C# and can be hosted by .NET applications. It supports
traditional database features like indexes, foreign key relations, transaction handling and isolation, stored procedures, query optimization.

## Getting started
* Install [NuGet package](https://www.nuget.org/packages/NMemory)
* Define the database

```csharp
public class Person
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class Group
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class MyDatabase : Database
{
    public MyDatabase()
    {
        var peopleTable = base.Tables.Create<Person, int>(p => p.Id);
        var groupTable = base.Tables.Create<Group, int>(g => g.Id);

        var peopleGroupIdIndex = peopleTable.CreateIndex(
            new RedBlackTreeIndexFactory<Person>(), 
            p => p.GroupId);

        this.Tables.CreateRelation(
            groupTable.PrimaryKeyIndex, 
            peopleGroupIdIndex, 
            x => x, 
            x => x);

        this.People = peopleTable;
        this.Groups = groupTable;
    }

    public ITable<Person> People { get; private set; }

    public ITable<Group> Groups { get; private set; }
}
```

* Create a database instance and some data

```csharp
MyDatabase db = new MyDatabase();

db.Groups.Insert(new Group { 
    Id = 1, 
    Name = "Alpha Group" });

db.Groups.Insert(new Group { 
    Id = 2, 
    Name = "Beta Group" });

db.People.Insert(new Person { 
    Id = 1, 
    Name = "John Doe", 
    GroupId = 1, 
    BirthDay = new DateTime(1966, 4, 12) });
```

* Perform queries

```csharp
var query =
    from p in db.People
    join g in db.Groups on p.GroupId equals g.Id
    select new { Name = p.Name, Group = g.Name };
    
query.ToList()
```

* Manipulate data

```csharp
var q = db.Groups.Where(x => x.Name.StartsWith("B"));

// Update command
q.Update(x => new Group { Name = x.Name + " (taged)" });

// Delete command
q.Delete();
```

## Useful links

- [Website](https://nmemory.net/)
- [Documentation](https://nmemory.net/overview)
- [Online Examples](https://nmemory.net/online-examples) 

## Contribute

The best way to contribute is by **spreading the word** about the library:

 - Blog it
 - Comment it
 - Star it
 - Share it
 
A **HUGE THANKS** for your help.

## More Projects

- Projects:
   - [EntityFramework Extensions](https://entityframework-extensions.net/)
   - [Dapper Plus](https://dapper-plus.net/)
   - [C# Eval Expression](https://eval-expression.net/)
- Learn Websites
   - [Learn EF Core](https://www.learnentityframeworkcore.com/)
   - [Learn Dapper](https://www.learndapper.com/)
- Online Tools:
   - [.NET Fiddle](https://dotnetfiddle.net/)
   - [SQL Fiddle](https://sqlfiddle.com/)
   - [ZZZ Code AI](https://zzzcode.ai/)
- and much more!

To view all our free and paid projects, visit [ZZZ Projects](https://zzzprojects.com/).
