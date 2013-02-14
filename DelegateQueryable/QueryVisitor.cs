using System.Linq;
using System.Linq.Expressions;

namespace DelegateQueryable
{
    public class QueryVisitor : System.Linq.Expressions.ExpressionVisitor, QueryInfo
    {
        public int Take { get; set; }
        public int Skip { get; set; }

        public void Process(Expression expression)
        {
            Visit((MethodCallExpression)expression);
        }

        // override ExpressionVisitor method
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if ((m.Method.DeclaringType == typeof(Queryable)) || (m.Method.DeclaringType == typeof(Enumerable)))
            {
                if (m.Method.Name.Equals("Skip"))
                {
                    Visit(m.Arguments[0]);

                    ConstantExpression countExpression = (ConstantExpression)(m.Arguments[1]);

                    Skip = ((int)countExpression.Value);

                    return m;
                }
                if (m.Method.Name.Equals("Take"))
                {
                    Visit(m.Arguments[0]);

                    ConstantExpression countExpression = (ConstantExpression)(m.Arguments[1]);

                    Take = ((int)countExpression.Value);
                    return m;
                }
            }

            return m;
        }
    }
}