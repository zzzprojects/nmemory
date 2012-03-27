using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;
using NMemory.Linq;

namespace NMemory.Common
{
    internal class QueryMethods
    {

        public static MethodInfo SelectMany
        {
            get
            {
                return GetMethodInfo(() =>

                    Queryable.SelectMany<object, object, object>(
                        null,
                        (x, y) => null,
                        (x, y) => null));
            }
        }

        public static MethodInfo First
        {
            get
            {
                return GetMethodInfo(() =>

                    Queryable.First<object>(null));
            }
        }

        public static MethodInfo Select
        {
            get
            {
                return GetMethodInfo(() =>

                    Queryable.Select<object, object>(
                        null,
                        x => null));
            }
        }

        public static MethodInfo SelectManyNoResultSelector
        {
            get
            {
                return GetMethodInfo(() =>

                    Queryable.SelectMany<object, object>(
                        null,
                        x => null));
            }
        }

        public static MethodInfo Join
        {
            get
            {
                return GetMethodInfo(() =>

                    Queryable.Join<object, object, object, object>(
                        null,
                        null,
                        x => null,
                        x => null,
                        (x, y) => null));
            }
        }

        public static MethodInfo LeftOuterJoin
        {
            get
            {
                return GetMethodInfo(() =>

                    QueryableEx.LeftOuterJoin<object, object, object, object>(
                        null,
                        null,
                        x => null,
                        x => null,
                        (x, y) => null));
            }
        }

        public static MethodInfo AsQueryable
        {
            get
            {
                return GetMethodInfo(() =>

                    Queryable.AsQueryable<object>(null));
            }
        }



        private static MethodInfo GetMethodInfo(Expression<Func<object>> expression)
        {
            MethodCallExpression methodCall = expression.Body as MethodCallExpression;

            if (methodCall == null)
            {
                return null;
            }

            return methodCall.Method.GetGenericMethodDefinition();
        }


    }
}
