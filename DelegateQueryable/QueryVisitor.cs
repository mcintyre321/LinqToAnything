using System.Linq;
using System.Linq.Expressions;

namespace DelegateQueryable
{
    public class QueryVisitor : System.Linq.Expressions.ExpressionVisitor, QueryInfo
    {
        private bool handled;
        public int? Take { get; set; }
        public int Skip { get; set; }

        public bool Process(Expression expression)
        {
            handled = false;
            Visit((MethodCallExpression)expression);
            return handled;
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
                    handled = true;
                    return m;
                }
                else if (m.Method.Name.Equals("Take"))
                {
                    Visit(m.Arguments[0]);

                    var countExpression = (ConstantExpression)(m.Arguments[1]);

                    Take = ((int)countExpression.Value);
                    handled = true;
                    return m;
                } 
            }

            return m;
        }
    }
}