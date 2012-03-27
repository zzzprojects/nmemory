using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace NMemory.Common
{
    internal static class TypeSystem
    {
        internal static Type GetUnderlyingIfNullable(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        internal static bool IsAnonymousType(Type type)
        {
            return
                type.GetInterface("IComparable") == null &&
                type.GetCustomAttributes(typeof(DebuggerDisplayAttribute), false)
                    .Cast<DebuggerDisplayAttribute>()
                    .Any(m => m.Type == "<Anonymous Type>");
        }


        internal static Type GetElementType( Type seqType )
        {

            Type ienum = FindIEnumerable( seqType );

            if( ienum == null ) return seqType;

            return ienum.GetGenericArguments()[0];

        }
        private static Type FindIEnumerable( Type seqType )
        {

            if( seqType == null || seqType == typeof( string ) )

                return null;

            if( seqType.IsArray )

                return typeof( IEnumerable<> ).MakeGenericType( seqType.GetElementType() );

            if( seqType.IsGenericType )
            {

                foreach( Type arg in seqType.GetGenericArguments() )
                {

                    Type ienum = typeof( IEnumerable<> ).MakeGenericType( arg );

                    if( ienum.IsAssignableFrom( seqType ) )
                    {

                        return ienum;

                    }

                }

            }

            Type[] ifaces = seqType.GetInterfaces();

            if( ifaces != null && ifaces.Length > 0 )
            {

                foreach( Type iface in ifaces )
                {

                    Type ienum = FindIEnumerable( iface );

                    if( ienum != null ) return ienum;

                }

            }

            if( seqType.BaseType != null && seqType.BaseType != typeof( object ) )
            {

                return FindIEnumerable( seqType.BaseType );

            }

            return null;

        }
    }
}