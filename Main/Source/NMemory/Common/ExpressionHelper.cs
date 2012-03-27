using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Common
{
    public static class ExpressionHelper
    {
        public static MemberExpression FindMemberExpression(Expression expression)
        {
            while (expression is UnaryExpression)
            {
                UnaryExpression unary = expression as UnaryExpression;

                expression = unary.Operand;
            }

            return expression as MemberExpression;
        }
    }
}
