using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common.Visitors;
using NMemory.StoredProcedures;

namespace NMemory.Common
{
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
