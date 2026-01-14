# Cmsql.Optimizely
This is the Optimizely CMS specific implementation for the [CMSQL - CMS Query Language](https://github.com/rbaarda/Cmsql). Please see the [CMSQL](https://github.com/rbaarda/Cmsql) repository for specifics on the CMS Query Language.

*Please note that this project is a POC and is still in alpha phase.*

## Getting started
On its own the Cmsql package can parse Cmsql queries but it needs a specific implementation to execute them.
This is the [Optimizely CMS](https://www.optimizely.com/products/content-management/) (formerly known as EPiServer) specific implementation which uses the [IPageCriteriaQueryService](https://world.optimizely.com/csclasslibraries/cms/EPiServer.Core.IPageCriteriaQueryService?version=12) at its core. You could say this implementation exposes the `IPageCriteriaQueryService` through the query language.

### Installation
You can install the NuGet package by running the following command:

`dotnet add package Cmsql.Optimizely --version 1.0.0`

### Usage
The Cmsql package contains a `CmsqlQueryService` which is basically a facade that takes care of parsing and executing queries through the `ExecuteQuery` method.
The `ExecuteQuery` method returns an instance of `CmsqlQueryResultSet` which is a composite type that contains information about the parsing and execution process.
When no errors are encountered and data is found the result set should contain data in the form of a collection of `ICmsqlQueryResult`.

The following (Optimizely CMS specific) example demonstrates how to execute a query, check for errors and get data from the result set.

```csharp
var resultSet = _cmsqlQueryService.ExecuteQuery("select ProductPage from start where PageName = 'Alloy Plan'");
if (!resultSet.ParseResult.Errors.Any() && !resultSet.ExecutionResult.Errors.Any())
{
  var pages = resultSet.ExecutionResult.QueryResults
    .OfType<PageDataCmsqlQueryResult>()
    .Select(p => p.Page)
    .ToList();
}
```
### Setup
The `CmsqlQueryService` needs to be given an instance of a `ICmsqlQueryRunner` through its constructor when instantiated. The specific `ICmsqlQueryRunner` implementation for this package is `PageCriteriaQueryRunner`. You are free to set this up yourself by for example StructureMap, this could look something like the following in your StructureMap configuration:
`x.For<ICmsqlQueryRunner>().Use<PageCriteriaQueryRunner>();`

## Limitations
Since the Cmsql.Optimizely package revolves around the `IPageCriteriaQueryService` it is also limited by it. For now, querying is limited to Pages (PageData) only, and one PageType per query.

For now, only the following operators are supported in query criteria:

* \= (Equals)
* \> (Greater than)
* \< (Less than)
* \!\= (Not equals)