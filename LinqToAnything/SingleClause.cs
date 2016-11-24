using System.Collections.Generic;
using System.Linq;

namespace LinqToAnything
{
    public class Single : ResultType
    {
        public override ResultType Clone()
        {
            return new Single();
        }
    }

    public abstract class ResultType
    {
        public abstract ResultType Clone();
    }
}