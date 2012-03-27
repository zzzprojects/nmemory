using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace NMemory.Tables
{
    internal class EntityPropertyCloner<TEntity>
    {
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
