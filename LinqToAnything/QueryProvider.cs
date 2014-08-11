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
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            queryVisitor.Visit(expression);
            if (typeof(TElement) != typeof(T))
            {
                DataQuery<TElement> q = info => _dataQuery(info).Select(queryVisitor.Transform<T, TElement>());
                return new DelegateQueryable<TElement>(q, null, queryVisitor);
            }
            return new DelegateQueryable<TElement>((DataQuery<TElement>)((object)_dataQuery), expression, queryVisitor);
 
        }


        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            var results = _dataQuery(queryVisitor.QueryInfo);
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
            var methodCallExpression = (MethodCallExpression)expression;
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            queryVisitor.Visit(expression);
            var array = _dataQuery(queryVisitor.QueryInfo).ToList();
            var data = array.AsQueryable();

            var newExp = Expression.Call(methodCallExpression.Method, Expression.Constant(data));
            return data.Provider.Execute<TResult>(newExp);
        }
    }

    public class SwitchoutArgumentVisitor : ExpressionVisitor
    {
        private readonly object arg;

        public SwitchoutArgumentVisitor(object arg)
        {
            this.arg = arg;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return Expression.Constant(arg);
        }
    }

    internal class SwitchoutVisitor<T> : ExpressionVisitor
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
            if (node.Value != null && node.Value is T)
            {
                return Expression.Constant(data);
            }
            return base.VisitConstant(node);
        }
    }
}