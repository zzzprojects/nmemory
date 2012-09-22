using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using NMemory.Tables;
using System.Reflection;
using NMemory.Utilities;

namespace NMemory.Common.Visitors
{
    internal class EntityTypeSearchVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo FindTable = 
            ReflectionHelper.GetMethodInfo<TableCollection>(t => t.FindTable(null));

        private static readonly MethodInfo FindTableGeneric =
            ReflectionHelper.GetStaticMethodInfo(() => TableCollectionExtensions.FindTable<object>(null));

        private HashSet<Type> entityTypes;

        public EntityTypeSearchVisitor()
        {
            this.entityTypes = new HashSet<Type>();
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            ITable table = node.Value as ITable;

            if (table != null)
            {
                this.entityTypes.Add(table.EntityType);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method == FindTable)
            {
                ConstantExpression nodeType = node.Arguments[0] as ConstantExpression;

                if (nodeType != null)
                {
                    this.entityTypes.Add(nodeType.Value as Type);
                }

                return null;
            }

            if (node.Method.IsGenericMethod && node.Method.GetGenericMethodDefinition() == FindTableGeneric)
            {
                this.entityTypes.Add(node.Method.GetGenericArguments()[0]);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Type type = ReflectionHelper.GetMemberUnderlyingType(node.Member);

            if (typeof(ITable).IsAssignableFrom(type))
            {
                // TODO: extract refactor
                Type[] possibleInterfaces = node.Type.GetInterfaces().Concat(new Type[] { node.Type }).ToArray();

                Type tableInterface =
                    possibleInterfaces.SingleOrDefault(
                        i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ITable<>));

                if (tableInterface != null)
                {
                    this.entityTypes.Add(tableInterface.GetGenericArguments()[0]);
                }
            }
            
            return base.VisitMember(node);
        }

        public Type[] FoundEntityTypes
        {
            get { return this.entityTypes.ToArray(); }
        }
    }
}
