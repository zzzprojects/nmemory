---
permalink: overview
---

## Definition

**NMemory** is a lightweight non-persistent in-memory relational database engine that is purely written in C# and can be hosted by .NET applications. It supports traditional database features; 

 - Indexes
 - Foreign Key Relations
 - Transaction Handling and Isolation
 - Stored Procedures
 - Query Optimization

NMemory library provides a class called Database which represents an in-memory database instance.

{% include template-example.html %} 
{% highlight csharp %}
public class MyDatabase : Database
{
    public MyDatabase()
    {
        var members = base.Tables.Create<Member, int>(p => p.Id);
        var groups = base.Tables.Create<Group, int>(g => g.Id);

       var peopleGroupIdIndex = members.CreateIndex(
           new RedBlackTreeIndexFactory<Member>(), 
           p => p.GroupId);

       this.Tables.CreateRelation(
           groups.PrimaryKeyIndex, 
           peopleGroupIdIndex, 
           x => x, 
           x => x);

        this.Members = members;
        this.Groups = groups;
    }

    public ITable<Member> Members { get; private set; }

    public ITable<Group> Groups { get; private set; }
}
{% endhighlight %}

[Learn more](/create-database)

Once the in-memory database is defined, create a database instance and insert some data into the in-memory database.

{% include template-example.html %} 
{% highlight csharp %}
MyDatabase db = new MyDatabase();

db.Groups.Insert(new Group { 
    Id = 1, 
    Name = "Alpha Group" });

db.Groups.Insert(new Group { 
    Id = 2, 
    Name = "Beta Group" });

db.Members.Insert(new Member { 
    Id = 1, 
    Name = "John Doe", 
    GroupId = 1, 
    BirthDay = new DateTime(1966, 4, 12) });

{% endhighlight %}

[Learn more](/create-table)

Currently, it just serves as the core component of the [Effort](http://entityframework-effort.net/) library. However, developer interest and contribution could make it a more robust engine that might serve well in a wide range of scenarios.

## Contribute

The best way to contribute is by spreading the word about the library:

 - Blog it
 - Comment it
 - Fork it
 - Star it
 - Share it
 - A **HUGE THANKS** for your help.

