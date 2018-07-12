// ----------------------------------------------------------------------------------
// <copyright file="EntityPropertyCloner.cs" company="NMemory Team">
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

namespace NMemory.Tables
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class EntityPropertyCloner<TEntity>
    {
        public static EntityPropertyCloner<TEntity> Instance = new EntityPropertyCloner<TEntity>();

        private Action<TEntity, TEntity> entityPropertyCloner;

        public EntityPropertyCloner()
        {
            ParameterExpression source = Expression.Parameter(typeof(TEntity));
            ParameterExpression destination = Expression.Parameter(typeof(TEntity));

            PropertyInfo[] properties = typeof(TEntity).GetProperties();
            Expression[] setters = new Expression[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                // destination.Property = source.Property;
                setters[i] = Expression.Call(destination, property.GetSetMethod(), Expression.Call(source, property.GetGetMethod()));
            }

            BlockExpression body = Expression.Block(setters);

            this.entityPropertyCloner = Expression.Lambda<Action<TEntity, TEntity>>(body, source, destination).Compile();
        }

        public void Clone(TEntity source, TEntity destination)
        {
            this.entityPropertyCloner.Invoke(source, destination);
        }
        
    }
}
