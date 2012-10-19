// ----------------------------------------------------------------------------------
// <copyright file="ExpressionHelper.cs" company="NMemory Team">
//     Copyright (C) 2012 by NMemory Team
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

namespace NMemory.Common
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common.Visitors;
    using NMemory.StoredProcedures;

    internal static class ExpressionHelper
    {
        public static MemberExpression FindMemberExpression(Expression expression)
        {
            while (expression is UnaryExpression)
            {
                UnaryExpression unary = expression as UnaryExpression;

                expression = unary.Operand;
            }

            return expression as MemberExpression;
        }

        public static Expression<Func<TEntity, TEntity>> ValidateAndCompleteUpdaterExpression<TEntity>(Expression<Func<TEntity, TEntity>> updater)
        {
            if (updater == null)
            {
                throw new ArgumentNullException("updater");
            }

            MemberInitExpression init = updater.Body as MemberInitExpression;

            if (init == null)
            {
                throw new ArgumentException("Invalid updater expression", "updater");
            }

            PropertyInfo[] properties = typeof(TEntity).GetProperties();
            MemberBinding[] bindings = new MemberBinding[properties.Length];
            ParameterExpression param = Expression.Parameter(typeof(TEntity));
            ParameterChangeVisitor parameterChanger = new ParameterChangeVisitor();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                MemberAssignment assign = init.Bindings.FirstOrDefault(b => b.Member == property) as MemberAssignment;
                Expression source = null;

                if (assign != null)
                {
                    source = parameterChanger.Change(assign.Expression, updater.Parameters[0], param);
                }
                else
                {
                    source = Expression.Property(param, property);
                }

                bindings[i] = Expression.Bind(property, source);
            }

            return
                Expression.Lambda<Func<TEntity, TEntity>>(
                    Expression.MemberInit(Expression.New(typeof(TEntity)), bindings),
                    param);
        }

        public static IParameter FindParameter(UnaryExpression node)
        {
            // The parameter is placed into the expression with the use of implicit conversion
            if (node.NodeType != ExpressionType.Convert || node.Method == null || node.Method.Name != "op_Implicit")
            {
                return null;
            }

            NewExpression init = node.Operand as NewExpression;

            // new Parameter<int>()
            if (init == null || !typeof(IParameter).IsAssignableFrom(init.Type))
            {
                return null;
            }

            // TODO: Performance
            IParameter parameter = Expression.Lambda<Func<IParameter>>(init).Compile()();

            return parameter;
        }
    }
}
