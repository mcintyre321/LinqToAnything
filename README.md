Linq To Anything
================

> Install-Package LinqToAnything

This bombastically titled library is, in a nutshell, an IQueryable<t>
wrapper for your non-iqueryable (but pageable)
data access methods, be they stored procedures or web requests or whatever.


Take one paged data access method:

```
    
    public void IEnumerable<Product> GetProductsFromSproc(int skip, int take, string category) { ... };
    
    class Product{
        public string Id {get;set;}
        public string Name {get;set;}
        public int Price {get;set;}
        public string Category {get;set;}
    }
```

Wrap it as a `Func<QueryInfo, IEnumerable<T>>` delegate


```

    Func<QueryInfo, IEnumerable<Product>> wrapper = queryInfo => {

        //extract the category value from the query info object
        var category = queryInfo.Clauses.OfType<Where>()
            .Where(c => c.PropertyName == "Category" && c.Operator == "Equals")
            .Select(c => c.Value);

        return GetProducts(queryInfo.Take, queryInfo.Skip, category);
    };


    //Now we can create our queryable using the wrapper delegate
    IQuerable<Product> queryable = new DelegateQueryable<string>(wrapper)

```

Basically, the queryInfo object provides a simplified view of the expression tree which you can then map to your data source:
A QueryInfo object contains:
    
  - a `Take`, a `Skip` and an `OrderBy` property 
  - an enumerable of `Clause` objects (`Where`s, or `Or`s).

Please see [QueryInfo.cs](https://github.com/mcintyre321/LinqToAnything/blob/master/LinqToAnything/QueryInfo.cs) for info on what queries are supported, and what operators you can use.
 
It makes it easy to disassemble and reconstruct Linq queries, so you can do custom stuff to them.
