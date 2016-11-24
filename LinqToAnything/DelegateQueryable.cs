using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToAnything
{
    public static class DelegateQueryable
    {
        public static IQueryable<TRes> Create<TRes>(Func<QueryInfo, object> dataQuery)
        {
            return new DelegateQueryable<TRes>(dataQuery);
        }
        public static IQueryable<TRes> Create<TRes>(Func<QueryInfo, IEnumerable<TRes>> dataQuery, Func<QueryInfo, int> countQuery = null)
        {
            return new DelegateQueryable<TRes>(qi =>
            {
                return dataQuery(qi);
            });
        }
    }
    public class DelegateQueryable<T> : IOrderedQueryable<T>
    {
        readonly QueryProvider<T> _provider;
        readonly Expression _expression;


        public DelegateQueryable(Func<QueryInfo, object> dataQuery)
        {
            this._provider = new QueryProvider<T>(dataQuery);
            this._expression = Expression.Constant(this);
        }
        internal DelegateQueryable(Func<QueryInfo, object> dataQuery, Expression expression, QueryVisitor ev)
        {
            
            this._provider = new QueryProvider<T>(dataQuery, ev);
            this._expression = expression ?? Expression.Constant(this);
            
        }

        Expression IQueryable.Expression => this._expression;

        Type IQueryable.ElementType => typeof(T);

        IQueryProvider IQueryable.Provider => this._provider;

        public IEnumerator<T> GetEnumerator() => this._provider.GetEnumerable<T>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
