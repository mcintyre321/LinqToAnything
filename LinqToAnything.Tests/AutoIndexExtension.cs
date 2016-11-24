using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LinqToAnything.HelperTypes;

namespace LinqToAnything.Tests
{
    public static class AutoIndexExtension
    {
        public static IQueryable<T> AutoIndex<T>(this IQueryable<T> items)
        {
            var getters = typeof(T).GetProperties()
                .Select(p => new {p.Name, func = new Func<T, object>(t => p.GetValue(t))})
                .ToDictionary(pair => pair.Name, pair => pair.func);
            Func<T, string, object> getValueFromObj = (o, s) => getters[s].Invoke(o);


            var lookups = new Dictionary<string, ILookup<object, T>>();
            var groupingLookups = new Dictionary<string, object>();

            return new DelegateQueryable<T>(qi =>
            {
                Func<IEnumerable<Where>> whereClauses =
                    () => qi.Clauses.OfType<Where>().Where(c => c.Operator == "Equal");


                IQueryable<T> filteredItems = items;
                List<string> propertyNames = new List<string>();

                for (var whereClause = whereClauses().FirstOrDefault();
                    whereClause != null;
                    whereClause = whereClauses().FirstOrDefault())
                {
                    propertyNames.Add(whereClause.PropertyName);
                    var lookupKey = string.Join(".", propertyNames);

                    filteredItems = LookupsForPropertyName(lookups, lookupKey, filteredItems, whereClause.PropertyName, whereClause.Value, getValueFromObj);
                    qi.Clauses = qi.Clauses.Except(new[] {whereClause}).ToArray();
                }

                var data = qi.ApplyTo(filteredItems);


                if (qi.GroupBy != null)
                {
                    if (!groupingLookups.ContainsKey(qi.GroupBy.KeyName))
                    {
                        var lookupKey = qi.GroupBy.KeyName;
                        if (!lookups.ContainsKey(lookupKey))
                        {
                            ILookup<object, T> lookup;
                            if (!lookups.TryGetValue(lookupKey, out lookup))
                            {
                                lookup = filteredItems.ToLookup(d => (object)getters[lookupKey].Invoke(d), d => d);
                                lookups[lookupKey] = lookup;
                            }
                        }
                        var groupings = lookups[lookupKey].Select(l => Grouping.Create(qi.GroupBy.KeyType, typeof(T), l.Key, l.Cast<object>()));

                        groupingLookups[qi.GroupBy.KeyName] = groupings.ToArray().AsQueryable().Cast<Grouping>();
                        
                    }

                    return groupingLookups[qi.GroupBy.KeyName];
                }

                return data;
            });
        }

        private static IQueryable<T> LookupsForPropertyName<T>(Dictionary<string, ILookup<object, T>> lookups, string lookupKey, IQueryable<T> filteredItems,
            string propertyName, object value, Func<T, string, object> getValueFromObj)
        {
            ILookup<object, T> lookup;
            if (!lookups.TryGetValue(lookupKey, out lookup))
            {
                lookup = filteredItems.ToLookup(d => getValueFromObj(d, propertyName), d => d);
                lookups[lookupKey] = lookup;
            }
            filteredItems = lookup[value].AsQueryable();
            return filteredItems;
        }
    }

  
}