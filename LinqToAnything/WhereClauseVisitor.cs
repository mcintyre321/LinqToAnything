using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToAnything
{
    public class WhereClauseVisitor : ExpressionVisitor
    {
        private List<Where> _filters = new List<Where>();
        public IEnumerable<Where> Filters { get { return _filters.AsReadOnly(); } }

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
               this.VisitBinary((BinaryExpression)node.Right);
               return node;
            }
            else
            {
                var filter = new Where();
                filter.PropertyName = ((MemberExpression) node.Left).Member.Name;
                filter.Operator = node.Method.Name;
                filter.Value = ((ConstantExpression) node.Right).Value;
                _filters.Add(filter);
                return node;
            }
        }
    }
}