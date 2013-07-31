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

Now use your queryable as you will. Note that LINQ operators other than Skip, Take or Select will not work. At some point 
the QueryInfo will be extended with more information about the query.
