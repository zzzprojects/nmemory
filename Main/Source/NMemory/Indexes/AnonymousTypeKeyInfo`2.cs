using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using NMemory.Common;

namespace NMemory.Indexes
{
    public class AnonymousTypeKeyInfo<TEntity, TKey> : KeyInfoBase<TEntity, TKey>
        where TEntity : class
    {
        public AnonymousTypeKeyInfo(Expression<Func<TEntity, TKey>> keySelector, SortOrder[] sortOrders) : 
            base(
                GetEntityKeyMembers(keySelector),
                sortOrders,

                new AnonymousTypeKeyComparer<TKey>(sortOrders),
                keySelector.Compile(),
                CreateKeyEmptinessDetector())
        {

        }

        public AnonymousTypeKeyInfo(Expression<Func<TEntity, TKey>> keySelector) :
            this(keySelector, GetDefaultSortOrders(keySelector))
        {

        }

        private static SortOrder[] GetDefaultSortOrders(Expression<Func<TEntity, TKey>> keySelector)
        {
            int fieldCount = GetEntityKeyMembers(keySelector).Length;

            return Enumerable.Repeat(SortOrder.Ascending, fieldCount).ToArray();
        }

        private static MemberInfo[] GetEntityKeyMembers(Expression<Func<TEntity, TKey>> keySelector)
        {
            NewExpression resultCreator = keySelector.Body as NewExpression;

            if (resultCreator == null)
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            if (!ReflectionHelper.IsAnonymousType(resultCreator.Type))
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            List<MemberInfo> result = new List<MemberInfo>();

            foreach (Expression arg in resultCreator.Arguments)
            {
                MemberExpression member = arg as MemberExpression;

                if (member == null)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }

                if (member.Expression.NodeType != ExpressionType.Parameter)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }

                result.Add(member.Member);
            }

            return result.ToArray();
        }

        private static Func<TKey, bool> CreateKeyEmptinessDetector()
        {
            Type keyType = typeof(TKey);
            PropertyInfo[] properties = keyType.GetProperties();

            ParameterExpression keyParam = Expression.Parameter(keyType);
            Expression body = null;

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                Type propertyType = property.PropertyType;

                if (ReflectionHelper.IsNullable(propertyType))
                {
                    Expression equalityTest =
                        Expression.Equal(
                            Expression.Property(keyParam, property),
                            Expression.Constant(null, propertyType));

                    if (body == null)
                    {
                        body = equalityTest;
                    }
                    else
                    {
                        body = Expression.Or(body, equalityTest);
                    }
                }
            }

            // If all the members of the anonymous type is not nullable, the body expression is null at this point
            if (body == null)
            {
                body = Expression.Constant(true);
            }

            Expression<Func<TKey, bool>> resultExpression =
                Expression.Lambda<Func<TKey, bool>>(body, keyParam);

            return resultExpression.Compile();
        }
    }
}
