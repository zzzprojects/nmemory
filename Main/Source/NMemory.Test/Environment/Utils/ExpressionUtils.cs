using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace NMemory.Test.Environment.Utils
{
    public static class ExpressionUtils
    {
        public static Expression<T> ChangeBody<T>(Expression<T> original, Expression newBody)
        {
            return Expression.Lambda<T>(newBody, original.Parameters);
        }
    }
}
