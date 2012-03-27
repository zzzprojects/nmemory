using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Execution.Optimization
{
    public interface IExpressionModifier
    {
        Expression ModifyExpression(Expression expression);
    }
}
