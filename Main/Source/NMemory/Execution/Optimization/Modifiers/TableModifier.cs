using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.Tables;
using NMemory.Common;
using System.Reflection;

namespace NMemory.Execution.Optimization.Modifiers
{
    public class TableModifier : ExpressionModifierBase
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is ITable)
            {
                return SelectTableExpression(node.Value);
            }

            return base.VisitConstant(node);
        }

        private static Expression SelectTableExpression(object obj)
        {
            ITable table = obj as ITable;

            if (table == null)
            {
                throw new InvalidOperationException("Constant is not a table.");
            }

            string selectAll = ReflectionHelper.GetMethodName<Table<object, object>>(x => x.SelectAll());

            MethodInfo selectAllMethod = table.GetType().GetMethod(selectAll, BindingFlags.NonPublic | BindingFlags.Instance);

            Expression result =
                Expression.Call(
                    QueryMethods.AsQueryable.MakeGenericMethod(table.EntityType),
                    Expression.Call(
                        Expression.Constant(table),
                        selectAllMethod)
                    );

            return result;
        }
    }
}
