// ----------------------------------------------------------------------------------
// <copyright file="EntityPropertyChangeDetector.cs" company="NMemory Team">
//     Copyright (C) 2012-2013 NMemory Team
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

namespace NMemory.Tables
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using NMemory.Common;

    internal class EntityPropertyChangeDetector<TEntity>
    {
        public static EntityPropertyChangeDetector<TEntity> Instance = new EntityPropertyChangeDetector<TEntity>();

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
