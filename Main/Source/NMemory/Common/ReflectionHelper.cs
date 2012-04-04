using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

namespace NMemory.Common
{
    internal class ReflectionHelper
    {
        //public static MethodInfo GetMethodInfo<T>(Expression<Func<T, object>> expression)
        //{
        //    var method =  expression.Body as MethodCallExpression;

        //    return method.Method;
        //}

        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            return method.Method;
        }

        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> expression)
        {
            var member = expression.Body as MemberExpression;

            return member.Member as PropertyInfo;
        }

        public static PropertyInfo GetPropertyInfo(Expression<Func<object>> expression)
        {
            var member = expression.Body as MemberExpression;

            return member.Member as PropertyInfo;
        }

        public static string GetMethodName<T>(Expression<Func<T, object>> expression)
        {
            var method = expression.Body as MethodCallExpression;

            return method.Method.Name;
        }


        public static Type GetUnderlyingIfNullable(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        public static bool IsAnonymousType(Type type)
        {
            return
                !typeof(IComparable).IsAssignableFrom(type) &&
                type.GetCustomAttributes(typeof(DebuggerDisplayAttribute), false)
                    .Cast<DebuggerDisplayAttribute>()
                    .Any(m => m.Type == "<Anonymous Type>");
        }
    }
}
