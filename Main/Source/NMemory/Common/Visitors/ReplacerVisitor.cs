using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace NMemory.Common.Visitors
{
    internal class ReplacerVisitor : ExpressionVisitor
    {
        public Func<Expression, bool> Condition { get; set; }
        public Func<Expression, Expression> NewExpression { get; set; }

        public ReplacerVisitor(Func<Expression, bool> condition, Func<Expression, Expression> newExpression)
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
