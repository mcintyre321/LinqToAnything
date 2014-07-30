using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToAnything
{
    public class QueryProvider<T> : IQueryProvider
    {
        private readonly DataQuery<T> _dataQuery;
        private QueryVisitor _ev;


        public QueryProvider(DataQuery<T> dataQuery, QueryVisitor ev = null)
        {
            _dataQuery = dataQuery;
            _ev = ev ?? new QueryVisitor();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var ev = _ev ?? (_ev = new QueryVisitor());
            ev.Process(expression);
            if (typeof(TElement) != typeof(T))
            {
                DataQuery<TElement> q = info => _dataQuery(info).Select(ev.Transform<T, TElement>());
                return new DelegateQueryable<TElement>(q, expression, _ev);
            }
            return new DelegateQueryable<TElement>((DataQuery<TElement>) ((object)_dataQuery), expression, _ev);
 
        }


        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            var queryVisitor = _ev ?? new QueryVisitor();
            var results = _dataQuery(queryVisitor);
            //if (queryVisitor.Select != null)
            //{
            //    var projectionFunc = (Func<T, TResult>)queryVisitor.Select.Lambda.Compile();
            //    return results.Select(projectionFunc);
            //}
            return (IEnumerable<TResult>) results;
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute<T>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var data =  _dataQuery(_ev).ToArray().AsQueryable();
            data = QueryInterceptor.QueryableExtensions.InterceptWith(data, new SwitchoutVisitor<DelegateQueryable<T>>(data));
            return (TResult) data.Provider.Execute(expression);
        }
    }

    public class SwitchoutVisitor<T> : ExpressionVisitor
    {
        private readonly object data;

        public SwitchoutVisitor(object data)
        {
            this.data = data;
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value.GetType() == typeof (T))
            {
                return Expression.Constant(data);
            }
            return base.VisitConstant(node);
        }
    }
}