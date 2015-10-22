// ----------------------------------------------------------------------------------
// <copyright file="DynamicMethodBuilder.cs" company="NMemory Team">
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

namespace NMemory.Common
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    internal delegate TObject DynamicPropertySetter<TObject>(TObject obj, params object[] args);

    internal static class DynamicMethodBuilder
    {
        public static Action<TObject, TValue> CreateSinglePropertySetter<TObject, TValue>(PropertyInfo member)
        {
            var objParam = Expression.Parameter(typeof(TObject));
            var valueParam = Expression.Parameter(typeof(TValue));

            var lambda = Expression.Lambda<Action<TObject, TValue>>(
                Expression.Call(objParam, member.GetSetMethod(), valueParam),
                objParam,
                valueParam);

            return lambda.Compile();
        }

        public static DynamicPropertySetter<TObject> CreatePropertySetter<TObject>(params PropertyInfo[] members)
        {
            DynamicMethod setterMethod = CreatePropertySetterImpl<TObject>(members);
            DynamicPropertySetter<TObject> setter = (DynamicPropertySetter<TObject>)setterMethod.CreateDelegate(typeof(DynamicPropertySetter<TObject>));

            return setter;
        }

        public static MethodInfo CreatePropertySetterMethodInfo<TObject>(params PropertyInfo[] members)
        {
            DynamicMethod setterDef = CreatePropertySetterImpl<TObject>(members);

            return setterDef;
        }

        private static DynamicMethod CreatePropertySetterImpl<TObject>(PropertyInfo[] members)
        {
            DynamicMethod setterDef = 
                new DynamicMethod(
                    typeof(TObject).Name + "PropertySetter",
                    typeof(TObject),
                    new Type[] { typeof(TObject), typeof(object[]) },
                    typeof(TObject),
                    true);

            ILGenerator setterIL = setterDef.GetILGenerator();

            for (int i = 0; i < members.Length; i++)
            {
                setterIL.Emit(OpCodes.Ldarg_0);
                setterIL.Emit(OpCodes.Ldarg_1);
                setterIL.Emit(OpCodes.Ldc_I4, i);
                setterIL.Emit(OpCodes.Ldelem_Ref);
                setterIL.Emit(OpCodes.Castclass, members[i].PropertyType);
                setterIL.Emit(OpCodes.Unbox_Any, members[i].PropertyType); 
                setterIL.Emit(OpCodes.Callvirt, members[i].GetSetMethod());
            }

            setterIL.Emit(OpCodes.Ldarg_0);
            setterIL.Emit(OpCodes.Ret);
            return setterDef;
        }
    }
}
