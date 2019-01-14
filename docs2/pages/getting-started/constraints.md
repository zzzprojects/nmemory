# Constraints

## Introduction

**Constraints** are used to specify rules for the data in a table. Constraints can be specified when the table is created with the **Constraints.Add** method.

 - Constraints are used to limit the type of data that can go into a table. 
 - This ensures the accuracy and reliability of the data on the table. 
 - If there is any violation of the constraint and the data action, the action is aborted.

## NChar Constraint

To limit the length of any string column, use **NCharConstraint** and pass the column name, and it's desired length as an argument.
 
{% include template-example.html %} 
```csharp
var members = this.Tables.Create<Member, string>(x => x.Id, null);
var groups = this.Tables.Create<Group, int>(x => x.Id, 
                new IdentitySpecification<Group>(x => x.Id, 1, 1));

// group name must not exceed 12 characters
groups.Contraints.Add(new NCharConstraint<Group>(x => x.Name, 12));
```
[Try it](https://dotnetfiddle.net/p7ztdf)

## Not Null Constraint

Ensures that a column cannot have a NULL value. Use **NotNullableConstraint** to make any column not nullable.

{% include template-example.html %} 
```csharp
var members = this.Tables.Create<Member, string>(x => x.Id, null);
var groups = this.Tables.Create<Group, int>(x => x.Id, 
                new IdentitySpecification<Group>(x => x.Id, 1, 1));

// group name can not be null
groups.Contraints.Add(new NotNullableConstraint<Group, string>(x => x.Name));
```
[Try it](https://dotnetfiddle.net/ai2ZZl)

