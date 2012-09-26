using System;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common;

namespace NMemory.Execution.Optimization.Rewriters
{
    public class TableScanRewriter : ExpressionRewriterBase
    {
        protected override Expression VisitConstant(ConstantExpression node)
        {
            // original:
            //
            //  DatabaseTable
            //
            // transformed:
            //  
            //  DatabaseTable.SelectAll<>
            //

            Type entityType = DatabaseReflectionHelper.GetTableEntityType(node.Type);

            if (entityType != null)
            {
                return CreateSelectTableExpression(node, entityType);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // original:
            //
            //  dbParam.FindTable<>
            //
            // transformed:
            //  
            //  dbParam.FindTable<>.SelectAll<>
            //

            Type entityType = DatabaseReflectionHelper.GetTableEntityType(node.Type);

            if (entityType != null && 
                node.Method.IsGenericMethod && 
                node.Method.GetGenericMethodDefinition() == DatabaseMembers.TableCollectionExtensions_FindTable)
            {
                return CreateSelectTableExpression(node, entityType);
            }

            return base.VisitMethodCall(node);
        }

        private static Expression CreateSelectTableExpression(Expression source, Type entityType)
        {
            MethodInfo selectAllMethod = DatabaseMembers.TableExtensions_SelectAll.MakeGenericMethod(entityType);

            Expression selectAllCall = Expression.Call(selectAllMethod, source);

            return Expression.Call(
                QueryMethods.AsQueryable.MakeGenericMethod(entityType),
                selectAllCall);
        }
    }
}
