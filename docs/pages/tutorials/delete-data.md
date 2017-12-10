---
permalink: delete-data
---

## Introduction

**Table.Delete** deletes an entity from the table. It takes the following arguments by using different overloads.

 - **entity**: An entity that contains the primary key of the entity to be deleted.
 - **transaction**: The transaction within which the delete operation executes.

## Delete Single Entity

To delete a single entity from a table, pass an entity that contains the primary key to be deleted as an argument in the Delete method.

{% include template-example.html %} 
{% highlight csharp %}

MyDatabase database = new MyDatabase();

database.Groups.Insert(new Group { Name = "Group 1" });
database.Groups.Insert(new Group { Name = "Group 2" });

database.Groups.Delete(new Group() { Id = 1 });

Group group = database.Groups.FirstOrDefault();

{% endhighlight %}

## Delete All

To delete all records from a table using the Delete() with no arguments.

{% include template-example.html %} 
{% highlight csharp %}

MyDatabase database = new MyDatabase();

database.Groups.Insert(new Group { Name = "Group 1" });
database.Groups.Insert(new Group { Name = "Group 2" });
database.Groups.Insert(new Group { Name = "Group 3" });

database.Groups.Delete();

Group group = database.Groups.FirstOrDefault();

{% endhighlight %}

