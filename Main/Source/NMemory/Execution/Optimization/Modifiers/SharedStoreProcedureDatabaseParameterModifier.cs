using System;
using System.Linq.Expressions;
using NMemory.Common;
using NMemory.Modularity;

namespace NMemory.Execution.Optimization.Modifiers
{
    public class SharedStoreProcedureDatabaseParameterModifier : ExpressionModifierBase
    {
        private TransformationContext context;

        public SharedStoreProcedureDatabaseParameterModifier(TransformationContext context)
        {
            this.context = context;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // original:
            //
            //  dbParam.DatabaseTable
            //
            // transformed:
            //  
            //  executionContextParam.Database.Tables.FindTable<T>
            //

            if (typeof(IDatabase).IsAssignableFrom(node.Member.DeclaringType) &&
                node.Expression is ParameterExpression)
            {
                Type entityType = DatabaseReflectionHelper.GetTableEntityType(node.Type);

                // Check if ITable<>
                if (entityType != null)
                {
                    Expression database = Expression.Property(this.context.Parameter, DatabaseMembers.ExecutionContext_Database);
                    Expression tables = Expression.Property(database, DatabaseMembers.Database_Tables);

                    return Expression.Call(DatabaseMembers.TableCollectionExtensions_FindTable.MakeGenericMethod(entityType), tables);
                }
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            // original:
            //
            //  dbParam.Database
            //
            // transformed:
            //  
            //  executionContextParam.Database
            //

            if (typeof(IDatabase).IsAssignableFrom(node.Type))
            {
                return Expression.Property(this.context.Parameter, DatabaseMembers.ExecutionContext_Database);
            }

            return base.VisitParameter(node);
        }
    }
}
