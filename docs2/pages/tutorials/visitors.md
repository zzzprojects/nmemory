# Visitors

## Introduction

NMemory library provides a class **EntityTypeSearchVisitor** which is inherited from **ExpressionVisitor** to create a more specialized class which requires traversing, examining or copying an expression tree.

### Search Constant Table

{% include template-example.html %} 
```csharp

EntityTypeSearchVisitor search = new EntityTypeSearchVisitor();

var database = new Database();
var table = database.Tables.Create<Member, string>(x => x.Id, null);

Expression expr = Expression.Constant(table);

search.Visit(expr);

```

### Search Index

{% include template-example.html %} 
```csharp
EntityTypeSearchVisitor search = new EntityTypeSearchVisitor();

var database = new Database();
var table = database.Tables.Create<Member, string>(x => x.Id, null);
var index = table.CreateIndex(new RedBlackTreeIndexFactory(), x => x.GroupId);

Expression expr = Expression.Constant(index);

search.Visit(expr);
```



