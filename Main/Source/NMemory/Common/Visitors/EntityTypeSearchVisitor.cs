using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NMemory.Tables;

namespace NMemory.Common.Visitors
{
    internal class EntityTypeSearchVisitor : ExpressionVisitor
    {
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
            if (node.Method == DatabaseMembers.TableCollection_FindTable)
            {
                ConstantExpression nodeType = node.Arguments[0] as ConstantExpression;

                if (nodeType != null)
                {
                    this.entityTypes.Add(nodeType.Value as Type);
                }

                return null;
            }

            if (node.Method.IsGenericMethod && node.Method.GetGenericMethodDefinition() == DatabaseMembers.TableCollectionExtensions_FindTable)
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
                Type entityType = DatabaseReflectionHelper.GetTableEntityType(node.Type);

                // Check if ITable<>
                if (entityType != null)
                {
                    this.entityTypes.Add(entityType);
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
