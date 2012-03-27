using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

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
    }
}
