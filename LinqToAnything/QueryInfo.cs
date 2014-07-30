using System.Collections;
using System.Collections.Generic;

namespace LinqToAnything
{
    public interface QueryInfo
    {
        int? Take { get; }
        int Skip { get; }
        OrderBy OrderBy { get; }
        IEnumerable<Filter> Filters { get; }
    }

    public class Filter 
    {
        public string ColumnName { get; internal set; }
        public string Operator { get; internal set; }
        public object Value { get; internal set; }
    }
}