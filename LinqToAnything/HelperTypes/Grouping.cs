using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LinqToAnything.HelperTypes
{
    public class Grouping
    {
        public object Key { get; }
        public IEnumerable<object> Elements { get; }

        protected Grouping(object key, IEnumerable<object> elements)
        {
            Key = key;
            Elements = elements;
        }

        public static object Create(Type type, Type elementType, object key, object elements)
        {
            var genericType = typeof(Grouping<,>).GetTypeInfo().MakeGenericType(type, elementType);
            return (Grouping)Activator.CreateInstance(genericType, key, elements);
        }
    }
    public class Grouping<TKey, TElement> : Grouping, IGrouping<TKey, TElement>
    {
        private readonly IEnumerable<TElement> _elements;

        public Grouping(TKey key, IEnumerable<TElement> elements) : base(key, elements.Cast<object>())
        {
            _elements = elements;
            Key = key;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public new TKey Key { get; }


    }
}