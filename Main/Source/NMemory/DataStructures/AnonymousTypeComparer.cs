using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Linq.Expressions;

namespace NMemory.DataStructures
{
    internal class AnonymousTypeComparer<T> : IComparer<T>
    {
        // TODO: Move to appropriate namespace

        private Func<T, T, int> comparer;

        public AnonymousTypeComparer()
        {
            var marker = typeof(T).GetCustomAttributes(typeof(DebuggerDisplayAttribute), false).Cast<DebuggerDisplayAttribute>();

            if (marker.All(m => m.Type != "<Anonymous Type>"))
            {
                throw new InvalidOperationException("The specified generic type is not an anonymous type");
            }

            PropertyInfo[] properties = typeof(T).GetProperties();

            if (properties.Length == 0)
            {
                throw new InvalidOperationException("No properties");
            }

            ParameterExpression x = Expression.Parameter(typeof(T), "x");
            ParameterExpression y = Expression.Parameter(typeof(T), "y");

            ParameterExpression variable = Expression.Variable(typeof(int), "var");

            List<Expression> blockBody = new List<Expression>();

            for (int i = 0; i < properties.Length; i++)
			{
                PropertyInfo p = properties[i];

			    if (i == 0)
	            {
		            blockBody.Add(
                        Expression.Assign(
                            variable, 
                            CreateComparsion(p, x, y)));
	            }
                else
                {
                    blockBody.Add(
                        Expression.IfThen(
                            Expression.Equal(variable, Expression.Constant(0)),
                                
                            Expression.Assign(variable, CreateComparsion(p, x, y))
                                
                            ));
                }
			}

            // Eval the last variable
            blockBody.Add(variable);

            var lambda = Expression.Lambda<Func<T, T, int>>(
                Expression.Block(
                    new ParameterExpression[] { variable },
                    blockBody),
                x,
                y);

            this.comparer = lambda.Compile();
        }

        private Expression CreateComparsion(PropertyInfo p, ParameterExpression x, ParameterExpression y)
        {
            var comparerType = typeof(Comparer<>).MakeGenericType(p.PropertyType);

            var comparer = comparerType
                .GetProperty("Default", BindingFlags.Static | BindingFlags.Public)
                .GetValue(null, null);

            var comparerMethod = comparerType
                .GetMethod("Compare");

            return Expression.Call(
                Expression.Constant(comparer), comparerMethod,
                
                Expression.Property(x, p),
                Expression.Property(y, p));
        }

        public int Compare(T x, T y)
        {
            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (x == null && y == null)
            {
                return 0;
            }

            int result = this.comparer(x, y);

            return result;
        }
    }
}
