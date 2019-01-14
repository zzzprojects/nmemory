# Create Relation

## Introduction

**TableCollection.CreateRelation** method creates a relationship between two tables when you want to associate rows of one table with rows of another table. It takes the following arguments.

 - **primaryIndex**: An IIndex that specifies the primary key.
 - **foreignIndex**: An IIndex that specifies the foreign key.
 - **convertForeignToPrimary**: A function to convert a foreign key to the corresponding primary key.
 - **convertPrimaryToForeign**: A function to convert a primary key to the corresponding foreign key.
 - **relationOptions**: An RelationOptions that specifies options of a relation.

To create an index, NMemory provides **Table.CreateIndex** method which creates a new index in a table and it takes two arguments.

 - **indexFactory**: The index factory.
 - **keySelector**: The expression representing the definition of the index key.

{% include template-example.html %} 	
```csharp

var memberIndex = members.CreateIndex(
    new RedBlackTreeIndexFactory<Member>(), 
    m => m.GroupId);

this.Tables.CreateRelation(
    groups.PrimaryKeyIndex, 
    memberIndex, 
    x => x, 
    x => x);

```
[Try it](https://dotnetfiddle.net/UymqaO)

The columns you choose for the foreign key must have the same data type of the primary columns they correspond to. 

