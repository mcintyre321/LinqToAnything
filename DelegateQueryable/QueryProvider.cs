using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DelegateQueryable
{
    public class QueryProvider<T> : IQueryProvider
    {
        private readonly DataQuery<T> _dataQuery;
        private QueryVisitor _ev;


        public QueryProvider(DataQuery<T> dataQuery, QueryVisitor ev = null)
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
            if (ev.Skip == 0 || ev.Take == 0)
            {
                return new DelegateQueryable<TElement>((DataQuery<TElement>)(object)_dataQuery, expression, _ev);
            }
            else
            {
                return _dataQuery(ev).Cast<TElement>().ToArray().AsQueryable();
            }
        }


        public IEnumerable<T> GetEnumerable()
        {
            return _dataQuery(_ev ?? new QueryVisitor());
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute<T>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return (TResult)_dataQuery(_ev).ToArray().AsQueryable().Provider.Execute(expression);
        }
    }

    public class ExecuteVisitor<T> : ExpressionVisitor
    {
        private readonly QueryProvider<T> _queryProvider;
        public object Result { get; set; }
        public ExecuteVisitor(QueryProvider<T> queryProvider)
        {
            _queryProvider = queryProvider;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {

            if (node.Method.Name == "Count")
            {
                Result = _queryProvider.GetEnumerable().Count();
            }
            else
            {

                return base.VisitMethodCall(node);
            }
            return node;
        }
    }
}