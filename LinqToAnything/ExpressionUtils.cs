using System.Linq.Expressions;

namespace LinqToAnything
{
    public static class ExpressionUtils
    {
        internal static Expression RemoveQuotes(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Quote)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            return expr;
        }

        /// <summary>Match result for a SelectCall</summary>
        public class SelectCallMatch
        {
            /// <summary>The method call expression represented by this match.</summary>
            public MethodCallExpression MethodCall { get; set; }

            /// <summary>The expression on which the Select is being called.</summary>
            public Expression Source { get; set; }

            /// <summary>The lambda expression being executed by the Select.</summary>
            public LambdaExpression Lambda { get; set; }

            /// <summary>The body of the lambda expression.</summary>
            public Expression LambdaBody { get; set; }
        }
    }
}