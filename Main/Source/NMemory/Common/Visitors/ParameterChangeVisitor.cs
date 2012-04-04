using System.Linq.Expressions;

namespace NMemory.Common.Visitors
{
    internal class ParameterChangeVisitor : ExpressionVisitor
    {
        private ParameterExpression from;
        private ParameterExpression to;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == from)
            {
                return to;
            }

            return base.VisitParameter(node);
        }

        public Expression Change(Expression expression, ParameterExpression from, ParameterExpression to)
        {
            this.from = from;
            this.to = to;

            return this.Visit(expression);
        }
    }
}
