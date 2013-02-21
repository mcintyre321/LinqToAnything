using System;
using System.Linq;
using System.Linq.Expressions;

namespace DelegateQueryable
{
    public class QueryVisitor : System.Linq.Expressions.ExpressionVisitor, QueryInfo
    {
        public int? Take { get; set; }
        public int Skip { get; set; }

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
}