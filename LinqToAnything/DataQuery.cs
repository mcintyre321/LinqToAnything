using System.Collections.Generic;

namespace LinqToAnything
{
    public delegate IEnumerable<T> DataQuery<out T>(QueryInfo info);
    public delegate int CountQuery(QueryInfo info);
}