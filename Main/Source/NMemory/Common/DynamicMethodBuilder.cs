using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace NMemory.Common
{
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
            DynamicMethod setterDef = new DynamicMethod(typeof(TObject).Name + "PropertySetter",
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
