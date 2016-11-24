using System.Linq.Expressions;

namespace LinqToAnything
{
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