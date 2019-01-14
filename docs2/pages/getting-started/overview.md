# Overview

## Definition

**NMemory** is a lightweight non-persistent in-memory relational database engine that is purely written in C# and can be hosted by .NET applications. It supports traditional database features; 

 - Indexes
 - Foreign Key Relations
 - Transaction Handling and Isolation
 - Stored Procedures
 - Query Optimization

Installing & Upgrading
Download the <a href="/download">NuGet Package</a>

NMemory library provides a class called Database which represents an in-memory database instance.

{% include template-example.html %} 
```csharp
public class MyDatabase : Database
{
	public MyDatabase()
	{
		var members = this.Tables.Create<Member, int>(x => x.Id);
		var groups = base.Tables.Create<Group, int>(g => g.Id);

		this.Members = members;
		this.Groups = groups;

		RelationOptions options = new RelationOptions(
			cascadedDeletion: true);

		var peopleGroupIdIndex = members.CreateIndex(
			new RedBlackTreeIndexFactory(),
			p => p.GroupId);

		this.Tables.CreateRelation(
			groups.PrimaryKeyIndex,
			peopleGroupIdIndex,
			x =>  x ?? -1,
			x => x,
			options);
	}

	public ITable<Member> Members { get; private set; }

	public ITable<Group> Groups { get; private set; }
}
```
[Try it](https://dotnetfiddle.net/O5Kil7)

[Learn more](/create-database)

Once the in-memory database is defined, create a database instance and insert some data into the in-memory database.

{% include template-example.html %} 
```csharp
MyDatabase myDatabase = new MyDatabase();

myDatabase.Groups.Insert(new Group() { Id = 1});

myDatabase.Groups.Insert(new Group() { Id = 2});

myDatabase.Members.Insert(new Member() { Id = 1, GroupId = 1, Name = "It's Member_1" });

myDatabase.Members.Insert(new Member() { Id = 2, Name = "It's Member_2" });

```
[Try it](https://dotnetfiddle.net/77UyeL)

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

