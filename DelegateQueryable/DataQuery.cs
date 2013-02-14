using System.Collections.Generic;

namespace DelegateQueryable
{
    public delegate IEnumerable<T> DataQuery<out T>(QueryInfo info);
}