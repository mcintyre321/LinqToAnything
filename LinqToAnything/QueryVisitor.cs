using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;

namespace LinqToAnything
{

    public class QueryVisitor : System.Linq.Expressions.ExpressionVisitor
    {
        public QueryInfo QueryInfo { get; private set; }


        public QueryVisitor(QueryInfo queryInfo = null)
        {
            this.QueryInfo = queryInfo ?? new QueryInfo();
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        // override ExpressionVisitor method
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if ((m.Method.DeclaringType == typeof (Queryable)) || (m.Method.DeclaringType == typeof (Enumerable)))
            {
                if (m.Method.Name.Equals("Skip"))
                {
                    Visit(m.Arguments[0]);

                    var countExpression = (ConstantExpression) (m.Arguments[1]);

                    QueryInfo.Skip = ((int) countExpression.Value);
                    return m;
                }
                else if (m.Method.Name.Equals("Take"))
                {
                    Visit(m.Arguments[0]);

                    var countExpression = (ConstantExpression) (m.Arguments[1]);

                    QueryInfo.Take = ((int) countExpression.Value);
                    return m;
                }
                else if (m.Method.Name.Equals("Select"))
                {
                    MethodCallExpression call = m;
                    LambdaExpression lambda = (LambdaExpression) ExpressionUtils.RemoveQuotes(call.Arguments[1]);
                    Expression body = ExpressionUtils.RemoveQuotes(lambda.Body);
                    Select = new ExpressionUtils.SelectCallMatch
                    {
                        MethodCall = call,
                        Source = call.Arguments[0],
                        Lambda = lambda,
                        LambdaBody = body
                    };
                }
                else if (m.Method.Name.Equals("OrderByDescending"))
                {
                    MethodCallExpression call = m;
                    var lambda = (LambdaExpression) ExpressionUtils.RemoveQuotes(call.Arguments[1]);
                    var lambdaBody = (MemberExpression) ExpressionUtils.RemoveQuotes(lambda.Body);
                    QueryInfo.OrderBy = new OrderBy(lambdaBody.Member.Name, OrderBy.OrderByDirection.Desc);
                }
                else if (m.Method.Name.Equals("OrderBy"))
                {
                    MethodCallExpression call = m;
                    var lambda = (LambdaExpression) ExpressionUtils.RemoveQuotes(call.Arguments[1]);
                    var lambdaBody = (MemberExpression) ExpressionUtils.RemoveQuotes(lambda.Body);
                    QueryInfo.OrderBy = new OrderBy(lambdaBody.Member.Name, OrderBy.OrderByDirection.Asc);
                }
                else if (m.Method.Name.Equals("Where"))
                {
                    MethodCallExpression call = m;
                    var whereClause = call.Arguments[1];
                    var whereClauseVisitor = new WhereClauseVisitor();
                    whereClauseVisitor.Visit(whereClause);
                    QueryInfo.Clauses = QueryInfo.Clauses.Concat((whereClauseVisitor.Filters)).ToArray();
                }

            }
            return m;
        }

        public ExpressionUtils.SelectCallMatch Select { get; set; }

        public Func<TIn, TOut> Transform<TIn, TOut>()
        {
            if (Select == null) return new Func<TIn, TOut>(i => (TOut) (object) i);
            return (Func<TIn, TOut>) Select.Lambda.Compile();
        }
    }

}