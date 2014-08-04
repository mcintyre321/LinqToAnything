using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToAnything
{
    public class WhereClauseVisitor : ExpressionVisitor
    {
        private List<Clause> _filters = new List<Clause>();

        public IEnumerable<Clause> Filters
        {
            get { return _filters.AsReadOnly(); }
        }

        private Expression lambdaExpression;
        private dynamic parameter;

        public override Expression Visit(Expression node)
        {
            if (lambdaExpression == null)
            {
                lambdaExpression = node;
                parameter = ((dynamic) lambdaExpression).Operand.Parameters[0];
            }
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var filter = new Where();
            var memberExpression = ((System.Linq.Expressions.MemberExpression) node.Object);
            filter.PropertyName = memberExpression.Member.Name;
            filter.Expression = Expression.Lambda(node, parameter);
            filter.Operator = node.Method.Name;
            filter.Value = node.Arguments[0];
            _filters.Add(filter);

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {
                this.Visit((BinaryExpression) node.Left);
                this.Visit((BinaryExpression) node.Right);
                return node;
            }
            else if (node.NodeType == ExpressionType.OrElse)
            {

                var filter = new Or {Operator = node.NodeType.ToString()};

                var whereVisitor = new WhereClauseVisitor();
                whereVisitor.lambdaExpression = this.lambdaExpression;

                whereVisitor.Visit(node.Left);
                whereVisitor.Visit(node.Right);
                filter.Clauses = whereVisitor.Filters;
                filter.Expression = Expression.Lambda(node, parameter);
                _filters.Add(filter);
                return node;
            }
            else
            {
                var filter = new Where();
                var member = node.Left as MemberExpression;

                if (member == null)
                {
                    var unaryMember = node.Left as UnaryExpression;
                    if (unaryMember != null)
                    {
                        member = unaryMember.Operand as MemberExpression;
                    }
                }

                if (member != null)
                {
                    filter.PropertyName = member.Member.Name;
                }
                filter.Expression = Expression.Lambda(node, parameter);
                filter.Operator = node.NodeType.ToString();
                filter.Value = GetValueFromExpression(node.Right);
                _filters.Add(filter);
                return node;
            }
        }
        private static object GetValueFromExpression(Expression node)
        {
            var member = node as MemberExpression;

            if (member == null)
            {
                var unaryMember = node as UnaryExpression;
                if (unaryMember != null)
                {
                    member = unaryMember.Operand as MemberExpression;
                }
            }

            if (member != null)
            {
                return Expression.Lambda(member).Compile().DynamicInvoke();
            }

            var constant = node as ConstantExpression;
            if (constant != null)
            {
                return constant.Value;
            }
            throw new NotImplementedException();
        }
    }
}