using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Indexes
{
    public interface IKeyInfoExpressionBuilder
    {
        Expression CreateKeyFactoryExpression(params Expression[] arguments);

        Expression CreateKeyMemberSelectorExpression(Expression source, int index);
    }
}
