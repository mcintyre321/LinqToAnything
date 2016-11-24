using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything.HelperTypes;
using System.Linq.Dynamic.Core;
using QueryInterceptor;

namespace LinqToAnything
{
    public class QueryProvider<T> : IQueryProvider
    {
        private readonly Func<QueryInfo, object> _dataQuery;
        private readonly QueryVisitor _queryVisitor;


        public QueryProvider(Func<QueryInfo, object> dataQuery, QueryVisitor queryVisitor = null)
        {
            _dataQuery = dataQuery;
            this._queryVisitor = queryVisitor ?? new QueryVisitor();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            queryVisitor.Visit(expression);
            var methodCallExpression = (MethodCallExpression)expression;
            if (methodCallExpression.Method.Name == "GroupBy")
            {
                var dataQueryResult = _dataQuery(queryVisitor.QueryInfo);

                //if (dataQueryResult is IEnumerable<TElement>)
                //    return ((IEnumerable<TElement>) dataQueryResult).AsQueryable();

                if (dataQueryResult is IEnumerable<Grouping> || dataQueryResult is IEnumerable<TElement>)
                {
                    var asEls = ((IEnumerable<object>)dataQueryResult).Cast<TElement>();
                    return asEls.AsQueryable();
                    
                    //return (IQueryable<TElement>) ungroupedData.GroupBy((Expression<Func<object, TElement>>)queryVisitor.QueryInfo.GroupBy.KeySelector.Source);
                    //return (IQueryable<TElement>)(object)groupBy;
                }
                var ungroupedData = (dataQueryResult as IEnumerable<T>)?.AsQueryable();
                if (ungroupedData != null)
                {
                    if (queryVisitor.QueryInfo.GroupBy.KeyName != null)
                        return ungroupedData.GroupBy(queryVisitor.QueryInfo.GroupBy.KeyName).Cast<TElement>();
                }


            }
            if (methodCallExpression.Method.Name == "Select")
            {
                Func<QueryInfo, object> q = info => ((IEnumerable)_dataQuery(info)).Cast<T>().Select(queryVisitor.Transform<T, TElement>());
                return new DelegateQueryable<TElement>(q, null, queryVisitor);
            }
            return new DelegateQueryable<TElement>(_dataQuery, expression, queryVisitor);
 
        }


        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            var results = _dataQuery(queryVisitor.QueryInfo);
            return (IEnumerable<TResult>) results;
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute<T>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            queryVisitor.Visit(expression);

            var dataQueryResult = _dataQuery(queryVisitor.QueryInfo);
            if (dataQueryResult is TResult)
            {
                return (TResult)dataQueryResult;
            }
            var methodCallExpression = (MethodCallExpression)expression;
            if (methodCallExpression.Method.Name == "Count")
            {
                if (dataQueryResult is IEnumerable)
                {
                    return (TResult)(object)((IEnumerable<object>)dataQueryResult).Count();
                }
                throw new NotImplementedException($"Cannot do {methodCallExpression.Method.Name}, return ${typeof(TResult)} or IEnumerable from query delegate");
            }
            if (methodCallExpression.Method.Name == "Single")
            {
                if (dataQueryResult is IEnumerable)
                    return (TResult)(object)((IEnumerable<object>)dataQueryResult).Single();

                throw new NotImplementedException($"Cannot do {methodCallExpression.Method.Name}, return ${typeof(TResult)} or IEnumerable from query delegate");
            }

            var array = ((IEnumerable)dataQueryResult).Cast<IEnumerable<object>>().ToList();
            var data = array.AsQueryable();

            var newExp = Expression.Call(methodCallExpression.Method, Expression.Constant(data));
            return data.Provider.Execute<TResult>(newExp);
        }
    }

    //public class SwitchoutArgumentVisitor : ExpressionVisitor
    //{
    //    private readonly object arg;

    //    public SwitchoutArgumentVisitor(object arg)
    //    {
    //        this.arg = arg;
    //    }

    //    protected override Expression VisitConstant(ConstantExpression node)
    //    {
    //        return Expression.Constant(arg);
    //    }
    //}

    //internal class SwitchoutVisitor<T> : ExpressionVisitor
    //{
    //    private readonly object data;

    //    public SwitchoutVisitor(object data)
    //    {
    //        this.data = data;
    //    }


    //    public override Expression Visit(Expression node)
    //    {
    //        return base.Visit(node);
    //    }

    //    protected override Expression VisitConstant(ConstantExpression node)
    //    {
    //        if (node.Value != null && node.Value is T)
    //        {
    //            return Expression.Constant(data);
    //        }
    //        return base.VisitConstant(node);
    //    }
    //}
}