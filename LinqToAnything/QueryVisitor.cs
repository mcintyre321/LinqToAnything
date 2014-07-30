using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;

namespace LinqToAnything
{
    public class QueryVisitor : System.Linq.Expressions.ExpressionVisitor, QueryInfo
    {
        private List<Filter> _filters = new List<Filter>();
        public int? Take { get; set; }
        public int Skip { get; set; }
        public OrderBy OrderBy { get; set; }

        public IEnumerable<Filter> Filters { get { return _filters.AsReadOnly(); } }

        public void Process(Expression expression)
        {
            Visit(expression);
            return;
        }

        // override ExpressionVisitor method
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if ((m.Method.DeclaringType == typeof(Queryable)) || (m.Method.DeclaringType == typeof(Enumerable)))
            {
                if (m.Method.Name.Equals("Skip"))
                {
                    Visit(m.Arguments[0]);

                    var countExpression = (ConstantExpression)(m.Arguments[1]);

                    Skip = ((int)countExpression.Value);
                    return m;
                }
                else if (m.Method.Name.Equals("Take"))
                {
                    Visit(m.Arguments[0]);

                    var countExpression = (ConstantExpression)(m.Arguments[1]);

                    Take = ((int)countExpression.Value);
                    return m;
                } else if(m.Method.Name.Equals("Select"))
                {
                    MethodCallExpression call = m;
                    LambdaExpression lambda = (LambdaExpression)ExpressionUtils.RemoveQuotes(call.Arguments[1]);
                    Expression body = ExpressionUtils.RemoveQuotes(lambda.Body);
                    Select =  new ExpressionUtils.SelectCallMatch
                    {
                        MethodCall = call,
                        Source = call.Arguments[0],
                        Lambda = lambda,
                        LambdaBody = body
                    };
                }else if (m.Method.Name.Equals("OrderByDescending"))
                {
                    MethodCallExpression call = m;
                    var lambda = (LambdaExpression)ExpressionUtils.RemoveQuotes(call.Arguments[1]);
                    var lambdaBody = (MemberExpression) ExpressionUtils.RemoveQuotes(lambda.Body);
                    OrderBy = new OrderBy(lambdaBody.Member.Name, OrderBy.OrderByDirection.Desc);
                }
                else if (m.Method.Name.Equals("Where"))
                {
                    MethodCallExpression call = m;
                    var whereClause = call.Arguments[1];
                    var whereClauseVisitor = new WhereClauseVisitor();
                    whereClauseVisitor.Visit(whereClause);
                    _filters.AddRange(whereClauseVisitor.Filters);
                }
            }

            return m;
        }

        public ExpressionUtils.SelectCallMatch Select { get; set; }

        public Func<TIn, TOut> Transform<TIn, TOut>()
        {
            if (Select == null) return new Func<TIn, TOut>(i => (TOut) (object) i);
            return (Func<TIn, TOut>)Select.Lambda.Compile();
        }
    }

    public class OrderBy
    {
        public OrderBy(string name, OrderByDirection direction)
        {
            Name = name;
            Direction = direction;
        }

        public enum OrderByDirection
        {
            Asc,Desc
        }
        public string Name { get; private set; }
        public OrderByDirection Direction { get; private set; }

    }
}