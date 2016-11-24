using System.Collections.Generic;

namespace LinqToAnything
{
    public class GroupByClause : Clause
    {
        public override IEnumerable<string> PropertyNames { get; }

        public GroupByClause(params string[] propertyNames) : this((IEnumerable<string>)propertyNames)
        {
            
        }
        public GroupByClause(IEnumerable<string> propertyNames)
        {
            PropertyNames = propertyNames;
        }

        public override Clause Clone()
        {
            return new GroupByClause(PropertyNames);
        }

    }
}