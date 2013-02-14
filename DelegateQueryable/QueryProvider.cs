using System;
using System.Linq;
using System.Linq.Expressions;

namespace DelegateQueryable
{
    public class QueryProvider<T> : IQueryProvider
    {
        private readonly DataQuery<T> _dataQuery;
        private QueryVisitor _ev;


        public QueryProvider(DataQuery<T> dataQuery,  QueryVisitor ev = null)
        {
            _dataQuery = dataQuery;
            _ev = ev;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var ev = _ev ?? (_ev = new QueryVisitor());
            ev.Process(expression);
            return new DelegateQueryable<TElement>((DataQuery<TElement>)(object)_dataQuery, expression, _ev);
        }

        public object Execute(Expression expression)
        {
            return _dataQuery(_ev);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}