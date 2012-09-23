using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NMemory.Common;

namespace NMemory.Tables
{
    internal class EntityPropertyChangeDetector<TEntity>
    {
        private Func<TEntity, TEntity, List<PropertyInfo>> entityPropertyChangeDetector; 

        public EntityPropertyChangeDetector()
        {
            ParameterExpression oldEntity = Expression.Parameter(typeof(TEntity));
            ParameterExpression newEntity = Expression.Parameter(typeof(TEntity));

            ParameterExpression result = Expression.Variable(typeof(List<PropertyInfo>));

            PropertyInfo[] properties = typeof(TEntity).GetProperties();

            Expression[] block = new Expression[properties.Length + 2];

            // result = new List<PropertyInfo>();
            block[0] = Expression.Assign(result, Expression.New(typeof(List<PropertyInfo>)));

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                MethodInfo compare = typeof(IEqualityComparer<>)
                    .MakeGenericType(property.PropertyType)
                    .GetMethod("Equals", new Type[] { property.PropertyType, property.PropertyType });
                
                PropertyInfo comparer = ReflectionHelper.GetPropertyInfo(() => EqualityComparer<object>.Default);
                object value = typeof(EqualityComparer<>)
                    .MakeGenericType(property.PropertyType)
                    .GetProperty(comparer.Name)
                    .GetValue(null, null);

                // if (oldEntity.Property == newEntity.Property)
                //    result.Add(Property);
                //
                block[1 + i] =
                    Expression.IfThen(
                        Expression.Not(Expression.Call(
                            Expression.Constant(value), 
                            compare, 
                            Expression.Property(oldEntity, property),
                            Expression.Property(newEntity, property))),
                        Expression.Call(
                            result,
                            ReflectionHelper.GetMethodInfo<List<PropertyInfo>>(x => x.Add(null)),
                            Expression.Constant(property)));
            }

            // return result;
            block[block.Length - 1] = result;

            BlockExpression body = Expression.Block(new ParameterExpression[] { result }, block);

            this.entityPropertyChangeDetector =
                Expression.Lambda<Func<TEntity, TEntity, List<PropertyInfo>>>(body, oldEntity, newEntity).Compile();
        }

        public List<PropertyInfo> GetChanges(TEntity originalEntity, TEntity newEntity)
        {
            return this.entityPropertyChangeDetector.Invoke(originalEntity, newEntity);
        }
    }
}
