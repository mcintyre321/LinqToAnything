using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToAnything
{
    public class WhereClauseVisitor : ExpressionVisitor
    {
        private List<Clause> _filters = new List<Clause>();
        public IEnumerable<Clause> Filters { get { return _filters.AsReadOnly(); } }

        private Expression lambdaExpression;

        public override Expression Visit(Expression node)
        {
            if (lambdaExpression == null)
            {
                lambdaExpression = node;
            }
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var filter = new Where();
            var memberExpression = ((System.Linq.Expressions.MemberExpression) node.Object);
            filter.PropertyName = memberExpression.Member.Name;
            filter.Operator = node.Method.Name;
            filter.Value = node.Arguments[0];
            _filters.Add(filter);

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {
                this.VisitBinary((BinaryExpression) node.Left);
                this.VisitBinary((BinaryExpression) node.Right);
                return node;
            }
            else if (node.NodeType == ExpressionType.OrElse)
            {
                var parameter = ((dynamic) lambdaExpression).Operand.Parameters[0];
                var filter = new Or {Operator = node.NodeType.ToString()};
                var whereVisitor = new WhereClauseVisitor();
                whereVisitor.VisitBinary((BinaryExpression)node.Left);
                whereVisitor.VisitBinary((BinaryExpression) node.Right);
                filter.Clauses = whereVisitor.Filters;
                filter.Expression = Expression.Lambda(node, parameter);
                _filters.Add(filter);
                return node;
            }
            else
            {
                var filter = new Where();
                filter.PropertyName = ((MemberExpression) node.Left).Member.Name;
                filter.Operator = node.NodeType.ToString();
                filter.Value = ((ConstantExpression) node.Right).Value;
                _filters.Add(filter);
                return node;
            }
        }
    }
}