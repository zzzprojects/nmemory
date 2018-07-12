// ----------------------------------------------------------------------------------
// <copyright file="EntityTypeSearchVisitor.cs" company="NMemory Team">
//     Copyright (C) NMemory Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// ----------------------------------------------------------------------------------

namespace NMemory.Common.Visitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using NMemory.Indexes;
    using NMemory.Tables;

    internal class EntityTypeSearchVisitor : ExpressionVisitor
    {
        private HashSet<Type> entityTypes;

        public EntityTypeSearchVisitor()
        {
            this.entityTypes = new HashSet<Type>();
        }

        public Type[] FoundEntityTypes
        {
            get { return this.entityTypes.ToArray(); }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            ITable table = node.Value as ITable;

            if (table != null)
            {
                this.entityTypes.Add(table.EntityType);
            }

            IIndex index = node.Value as IIndex;

            if (index != null)
            {
                this.entityTypes.Add(index.Table.EntityType);
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

            if (node.Method.IsGenericMethod && 
                node.Method.GetGenericMethodDefinition() == 
                    DatabaseMembers.TableCollectionExtensions_FindTable)
            {
                this.entityTypes.Add(node.Method.GetGenericArguments()[0]);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Type type = ReflectionHelper.GetMemberType(node.Member);

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
    }
}
