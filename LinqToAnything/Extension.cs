using System.Linq;

namespace LinqToAnything
{
    public static class Extension
    {
        public static IQueryable<T> ToDelegateQueryable<T>(this IQueryable<T> inner)
        {
            DataQuery<T> query = info =>
            {
                var q = inner;
                if (info.Skip > 0) q = q.Skip(info.Skip);
                if (info.Take.HasValue) q = q.Take(info.Take.Value);
                return q;
            };
            return new DelegateQueryable<T>(query);
        } 
    }
}
