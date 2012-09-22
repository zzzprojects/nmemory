using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.Modularity;
using NMemory.Common;
using System.Reflection;
using NMemory.Tables;
using NMemory.Utilities;

namespace NMemory.Execution.Optimization.Modifiers
{
    public class SharedStoreProcedureDatabaseParameterModifier : ExpressionModifierBase
    {
        private TransformationContext context;

        private static readonly MethodInfo FindTable =
            ReflectionHelper.GetStaticMethodInfo(() => TableCollectionExtensions.FindTable<object>(null)).GetGenericMethodDefinition();

        private static readonly PropertyInfo ExecutionContextDatabase =
            ReflectionHelper.GetPropertyInfo<IExecutionContext, IDatabase>(c => c.Database);

        private static readonly PropertyInfo DatabaseTableCollection =
            ReflectionHelper.GetPropertyInfo<IDatabase, TableCollection>(d => d.Tables);

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
                // TODO : extract refactor
                Type[] possibleInterfaces = node.Type.GetInterfaces().Concat(new Type[] { node.Type }).ToArray();

                Type tableInterface =
                    possibleInterfaces.SingleOrDefault(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(ITable<>));

                if (tableInterface != null)
                {
                    Type entityType = tableInterface.GetGenericArguments()[0];

                    Expression database = Expression.Property(this.context.Parameter, ExecutionContextDatabase);
                    Expression tables = Expression.Property(database, DatabaseTableCollection);

                    return Expression.Call(FindTable.MakeGenericMethod(entityType), tables);
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
                return Expression.Property(this.context.Parameter, ExecutionContextDatabase);
            }

            return base.VisitParameter(node);
        }
    }
}
