using System.Collections.Generic;
using System.Linq;

namespace LinqToAnything
{
    public class Count : ResultType
    {
        public override ResultType Clone()
        {
            return new Count();
        }

    }
}