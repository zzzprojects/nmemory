using System;
using System.Linq.Expressions;

namespace NMemory.Common.Visitors
{
    internal class ReplaceVisitor : ExpressionVisitor
    {
        public Func<Expression, bool> Condition { get; set; }
        public Func<Expression, Expression> NewExpression { get; set; }

        public ReplaceVisitor(Func<Expression, bool> condition, Func<Expression, Expression> newExpression)
        {
            this.Condition = condition;
            this.NewExpression = newExpression;
        }

        public override Expression Visit(Expression exp)
        {
            if (Condition(exp))
            {
                return NewExpression(exp);
            }
            else
            {
                return base.Visit(exp);
            }
        }
    }
}
