using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using QueryInterceptor;

namespace LinqToAnything
{
    public class QueryProvider<T> : IQueryProvider
    {
        private readonly DataQuery<T> _dataQuery;
        private QueryVisitor _queryVisitor;


        public QueryProvider(DataQuery<T> dataQuery, QueryVisitor _queryVisitor = null)
        {
            _dataQuery = dataQuery;
            this._queryVisitor = _queryVisitor ?? new QueryVisitor();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var queryVisitor = _queryVisitor ?? (_queryVisitor = new QueryVisitor());
            queryVisitor.Visit(expression);
            if (typeof(TElement) != typeof(T))
            {
                DataQuery<TElement> q = info => _dataQuery(info).Select(queryVisitor.Transform<T, TElement>());
                return new DelegateQueryable<TElement>(q, null, _queryVisitor);
            }
            return new DelegateQueryable<TElement>((DataQuery<TElement>) ((object)_dataQuery), expression, _queryVisitor);
 
        }


        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            var queryVisitor = _queryVisitor ?? new QueryVisitor();
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
            var data =  _dataQuery(new QueryVisitor()).ToArray().AsQueryable();
            data = data.InterceptWith(new SwitchoutVisitor<DelegateQueryable<T>>(data));
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