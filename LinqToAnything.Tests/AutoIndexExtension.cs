using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToAnything.Tests
{
    public static class AutoIndexExtension
    {
        public static IQueryable<T> AutoIndex<T>(this IQueryable<T> items)
        {
            var getters = typeof(T).GetProperties()
                .Select(p => new {p.Name, func = new Func<T, object>(t => p.GetValue(t))})
                .ToDictionary(pair => pair.Name, pair => pair.func);


            var lookups = new Dictionary<string, ILookup<object, T>>();
            return new DelegateQueryable<T>(qi =>
            {
                Func<IEnumerable<Where>> whereClauses =
                    () => qi.Clauses.OfType<Where>().Where(c => c.Operator == "Equal");
                IQueryable<T> filteredItems = items;
                for (var whereClause = whereClauses().FirstOrDefault();
                    whereClause != null;
                    whereClause = whereClauses().FirstOrDefault())
                {
                    ILookup<object, T> lookup;
                    if (!lookups.TryGetValue(whereClause.PropertyName, out lookup))
                    {

                        lookup = filteredItems.ToLookup(d => (object) getters[whereClause.PropertyName].Invoke(d),
                            d => d);
                        lookups[whereClause.PropertyName] = lookup;
                    }
                    filteredItems = lookup[whereClause.Value].AsQueryable();
                    qi.Clauses = qi.Clauses.Except(new[] {whereClause}).ToArray();
                }
                return qi.ApplyTo(filteredItems);
            });
        }
    }
}