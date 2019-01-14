# Create Table

## Introduction
 
**NMemory** provides a class **TableCollection** which represents a collection of the tables in a database. **TableCollection.Create** method initializes a database table and it takes two arguments.

 - **primaryKey**: An IKeyInfo object that represents the primary key of the entities.
 - **identitySpecification**: An IdentitySpecification to set an identity field.

{% include template-example.html %} 
```csharp
Database database = new Database();

database.Tables.Create<Group, int>(g => g.Id, null);
```
[Try it](https://dotnetfiddle.net/Ob2ZMJ)

If your class is inherited from the Database class, call Create method in your class constructor. 

{% include template-example.html %} 
```csharp
public class MyDatabase : Database
{
    public MyDatabase()
    {
        var members = this.Tables.Create<Member, string>(x => x.Id, null);
        var groups = base.Tables.Create<Group, int>(g => g.Id, null);

       this.Members = members;
       this.Groups = groups;
    }

    public ITable<Member> Members { get; private set; }

    public ITable<Group> Groups { get; private set; }
}
```
[Try it](https://dotnetfiddle.net/Uc0cOP)

Now when object is created of MyDatabase class, it will also create two tables in that in-memory database. 

## Identity Column

To create an identity column in a table, pass **IdentitySpecification** object in the second argument of Create method. IdentitySpecification constructor takes three arguments

 - **identityColumn**: Property field which needs to be identity column in a table.
 - **seed**: Is the value that is used for the very first row loaded into the table.
 - **increment**: Is the incremental value that is added to the identity value of the previous row that was loaded.

{% include template-example.html %} 
```csharp

var groups = base.Tables.Create<Group, int>(g => g.Id, 
                    new IdentitySpecification<Group>(x => x.Id, 1, 1) );

```
[Try it](https://dotnetfiddle.net/I0louZ)


