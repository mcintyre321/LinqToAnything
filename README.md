Linq To Anything
================

> Install-Package LinqToAnything

This bombastically titled library is, in a nutshell, an IQueryable<T> wrapper for your non-iqueryable (but pageable) 
data access methods, be they stored procedures or web requests or whatever. 


Take one paged data access method:

```
public void IEnumerable<Product> GetProducts(int pageIndex, int pageSize);
```

Wrap it as a [DataQuery](https://github.com/mcintyre321/LinqToAnything/blob/master/LinqToAnything/DataQuery.cs) delegate:

```
DataQuery<Product> dataQuery = queryInfo => GetProducts(queryInfo.Take / queryInfo.Skip, queryInfo.Skip);
                                                         //notice that we have mapped Skip and Take into paging parameters

IQuerable<Product> queryable = new DelegateQueryable<string>(dataQuery)
```

Basically, the queryInfo object provides a simplified view of the expression tree which you can then map to your data source.

Please see [QueryInfo.cs](https://github.com/mcintyre321/LinqToAnything/blob/master/LinqToAnything/QueryInfo.cs) for info on what queries are supported, and what operators you can use.

Skip, Take, Orderby and simple Where clauses are supported (enough to do Datatables)

