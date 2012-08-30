using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common;
using System.Collections.ObjectModel;

namespace NMemory.Indexes
{
    public class TupleKeyInfo<TEntity, TKey> : KeyInfoBase<TEntity, TKey>
        where TEntity : class
    {
        public TupleKeyInfo(Expression<Func<TEntity, TKey>> keySelector, SortOrder[] sortOrders) : 
            base(
                GetEntityKeyMembers(keySelector),
                sortOrders,
                new TupleKeyComparer<TKey>(sortOrders),
                keySelector.Compile(),
                TupleKeyInfo<TEntity, TKey>.CreateKeyEmptinessDetector())
        {

        }

        public TupleKeyInfo(Expression<Func<TEntity, TKey>> keySelector) :
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
            if (!ReflectionHelper.IsTuple(keySelector.Body.Type))
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            NewExpression newTupleExpression = keySelector.Body as NewExpression;

            if (newTupleExpression != null)
            {
                return GetMemberInfoFromArguments(newTupleExpression.Arguments);
            }

            MethodCallExpression createTupleExpression = keySelector.Body as MethodCallExpression;

            if (createTupleExpression != null)
            {
                MethodInfo createMethod = createTupleExpression.Method;

                if (createMethod.DeclaringType != typeof(Tuple) || createMethod.Name != "Create")
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }

                return GetMemberInfoFromArguments(createTupleExpression.Arguments);
            }

            throw new ArgumentException("Invalid expression", "keySelector");
        }

        private static MemberInfo[] GetMemberInfoFromArguments(ReadOnlyCollection<Expression> arguments)
        {
            MemberInfo[] result = new MemberInfo[arguments.Count];

            for (int i = 0; i < arguments.Count; i++)
			{
                MemberExpression member = arguments[i] as MemberExpression;

                if (member == null)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }

                if (member.Expression.NodeType != ExpressionType.Parameter)
                {
                    throw new ArgumentException("Invalid expression", "keySelector");
                }

                result[i] = member.Member;
			}

            return result;
        }

        private static Func<TKey, bool> CreateKeyEmptinessDetector()
        {
            if (!ReflectionHelper.IsTuple(typeof(TKey)))
            {
                throw new ArgumentException("Invalid expression", "keySelector");
            }

            Type keyType = typeof(TKey);
            PropertyInfo[] properties = keyType.GetProperties();

            ParameterExpression keyParam = Expression.Parameter(keyType);
            Expression body = null;

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                Type propertyType = property.PropertyType;

                if (!property.Name.StartsWith("Item", StringComparison.InvariantCulture))
                {
                    continue;
                }

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

            // If all the members of the tuple is not nullable, the body expression is null at this point
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
