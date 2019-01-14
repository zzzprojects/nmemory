# Create Database

## In-Memory Database

An in-memory database primarily relies on main memory for computer data storage. 

 - Main memory databases are faster than disk-optimized databases because disk access is slower than memory access, the internal optimization algorithms are simpler and execute fewer CPU instructions. 
 - Accessing data in memory eliminates seek-time when querying the data, which provides faster and more predictable performance than disk.

This tutorial shows a fundamental approach to create an in-memory database for a very simple model using NMemory.

{% include template-example.html %} 
```csharp

public class Member
{
    public string Id { get; set; }

    public string Name { get; set; }

    public int? GroupId { get; set; }

    public int GroupId2 { get; set; }
}

public class Group
{
    public int Id { get; set; }

    public int Id2 { get; set; }

    public string Name { get; set; }

}

```
[Try it](https://dotnetfiddle.net/xBxpYE)

NMemory library provides a class called Database which represents an in-memory database instance which can contain tables and stored procedures. The simplest way is to create an object of Database class.

{% include template-example.html %} 
```csharp

Database database = new Database();

```
[Try it](https://dotnetfiddle.net/3Kk3hj)

You can also inherit Database class to define your own in-memory database.

{% include template-example.html %} 
```csharp
public class MyDatabase : Database
{
    public MyDatabase()
    {
    }

    public ITable<Member> Members { get; private set; }

    public ITable<Group> Groups { get; private set; }
}
```
[Try it](https://dotnetfiddle.net/Xx5lta)

And then create an object of your own class.

{% include template-example.html %} 
```csharp

MyDatabase myDatabase = new MyDatabase();

```
[Try it](https://dotnetfiddle.net/qvQz0e)

The database is created, but it doesn't contain any table or stored procedure right now.


