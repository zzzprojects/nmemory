using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Reflection.Emit;

namespace NMemory.Common
{
	public static class EntityHelper
	{
		/// <summary>
		/// This dictionary caches the delegates for each 'to-clone' type.
		/// </summary>
		static Dictionary<Type, Delegate> _cachedIL = new Dictionary<Type, Delegate>();

		/// <summary>
		/// Átmásolja a from objektumban lévő propertyk értékeit a to objektum azonos propertijeinek értékeire.
		/// </summary>
		/// <param name="objCopyFrom"></param>
		/// <param name="objCopyTo"></param>
		public static void Copy<TFrom, TTo>(TFrom objCopyFrom, TTo objCopyTo)
		{
			foreach(PropertyInfo info in typeof(TFrom).GetProperties())
			{
				PropertyInfo prop = typeof(TTo).GetProperty(info.Name);

				if(prop != null && prop.PropertyType == info.PropertyType && prop.CanWrite)
				{
					object v = info.GetValue(objCopyFrom, null);
					prop.SetValue(objCopyTo, v, null);
				}

			}
		}

		///// <summary>
		///// 
		///// </summary>
		///// <returns></returns>
		//public static object Clone(object toClone)
		//{
		//    //First we create an instance of this specific type.

		//    object newObject = Activator.CreateInstance(toClone.GetType());

		//    //We get the array of fields for the new type instance.

		//    FieldInfo[] fields = newObject.GetType().GetFields();

		//    int i = 0;

		//    foreach (FieldInfo fi in toClone.GetType().GetFields())
		//    {
		//        //We query if the fiels support the ICloneable interface.

		//        Type ICloneType = fi.FieldType.
		//                    GetInterface("ICloneable", true);

		//        if (ICloneType != null)
		//        {
		//            //Getting the ICloneable interface from the object.

		//            ICloneable IClone = (ICloneable)fi.GetValue(toClone);

		//            //We use the clone method to set the new value to the field.

		//            fields[i].SetValue(newObject, IClone.Clone());
		//        }
		//        else
		//        {
		//            // If the field doesn't support the ICloneable 

		//            // interface then just set it.

		//            fields[i].SetValue(newObject, fi.GetValue(toClone));
		//        }

		//        //Now we check if the object support the 

		//        //IEnumerable interface, so if it does

		//        //we need to enumerate all its items and check if 

		//        //they support the ICloneable interface.

		//        Type IEnumerableType = fi.FieldType.GetInterface
		//                        ("IEnumerable", true);
		//        if (IEnumerableType != null)
		//        {
		//            //Get the IEnumerable interface from the field.

		//            IEnumerable IEnum = (IEnumerable)fi.GetValue(toClone);

		//            //This version support the IList and the 

		//            //IDictionary interfaces to iterate on collections.

		//            Type IListType = fields[i].FieldType.GetInterface
		//                                ("IList", true);
		//            Type IDicType = fields[i].FieldType.GetInterface
		//                                ("IDictionary", true);

		//            int j = 0;
		//            if (IListType != null)
		//            {
		//                //Getting the IList interface.

		//                IList list = (IList)fields[i].GetValue(newObject);

		//                foreach (object obj in IEnum)
		//                {
		//                    //Checking to see if the current item 

		//                    //support the ICloneable interface.

		//                    ICloneType = obj.GetType().
		//                        GetInterface("ICloneable", true);

		//                    if (ICloneType != null)
		//                    {
		//                        //If it does support the ICloneable interface, 

		//                        //we use it to set the clone of

		//                        //the object in the list.

		//                        ICloneable clone = (ICloneable)obj;

		//                        list[j] = clone.Clone();
		//                    }

		//                    //NOTE: If the item in the list is not 

		//                    //support the ICloneable interface then in the 

		//                    //cloned list this item will be the same 

		//                    //item as in the original list

		//                    //(as long as this type is a reference type).


		//                    j++;
		//                }
		//            }
		//            else if (IDicType != null)
		//            {
		//                //Getting the dictionary interface.

		//                IDictionary dic = (IDictionary)fields[i].
		//                                    GetValue(newObject);
		//                j = 0;

		//                foreach (DictionaryEntry de in IEnum)
		//                {
		//                    //Checking to see if the item 

		//                    //support the ICloneable interface.

		//                    ICloneType = de.Value.GetType().
		//                        GetInterface("ICloneable", true);

		//                    if (ICloneType != null)
		//                    {
		//                        ICloneable clone = (ICloneable)de.Value;

		//                        dic[de.Key] = clone.Clone();
		//                    }
		//                    j++;
		//                }
		//            }
		//        }
		//        i++;
		//    }
		//    return newObject;
		//}

		/// <summary>
		/// Generic cloning method that clones an object using IL.
		/// Only the first call of a certain type will hold back performance.
		/// After the first call, the compiled IL is executed.
		/// </summary>
		/// <typeparam name="T">Type of object to clone</typeparam>
		/// <param name="myObject">Object to clone</param>
		/// <returns>Cloned object</returns>
		public static T Clone<T>(T myObject)
		{
			Delegate myExec = null;
			if(!_cachedIL.TryGetValue(typeof(T), out myExec))
			{
				// Create ILGenerator
				DynamicMethod dymMethod = new DynamicMethod("DoClone", typeof(T), new Type[] { typeof(T) }, true);
				ConstructorInfo cInfo = myObject.GetType().GetConstructor(new Type[] { });

				ILGenerator generator = dymMethod.GetILGenerator();

				LocalBuilder lbf = generator.DeclareLocal(typeof(T));
				//lbf.SetLocalSymInfo("_temp");

				generator.Emit(OpCodes.Newobj, cInfo);
				generator.Emit(OpCodes.Stloc_0);
				foreach(FieldInfo field in myObject.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
				{
					// Load the new object on the eval stack... (currently 1 item on eval stack)
					generator.Emit(OpCodes.Ldloc_0);
					// Load initial object (parameter)          (currently 2 items on eval stack)
					generator.Emit(OpCodes.Ldarg_0);
					// Replace value by field value             (still currently 2 items on eval stack)
					generator.Emit(OpCodes.Ldfld, field);
					// Store the value of the top on the eval stack into the object underneath that value on the value stack.
					//  (0 items on eval stack)
					generator.Emit(OpCodes.Stfld, field);
				}

				// Load new constructed obj on eval stack -> 1 item on stack
				generator.Emit(OpCodes.Ldloc_0);
				// Return constructed object.   --> 0 items on stack
				generator.Emit(OpCodes.Ret);

				myExec = dymMethod.CreateDelegate(typeof(Func<T, T>));
				_cachedIL.Add(typeof(T), myExec);
			}
			return ((Func<T, T>)myExec)(myObject);
		}

		//public static T Update<T, K>(T originalEntity, Func<T, K> updater)
		//{
			/* .method public hidebysig instance class MMDB.Test.Employee 
				Update(class MMDB.Test.Employee employeeToUpdate, int32 ID) cil managed
			{
			  // Code size       22 (0x16)
			  .maxstack  2
			  .locals init ([0] class MMDB.Test.Employee e,
					   [1] class MMDB.Test.Employee CS$1$0000)
			  IL_0000:  nop
			  IL_0001:  ldarg.1
			  IL_0002:  call       !!0 [MMDB]MMDB.EntityHelper::Clone<class MMDB.Test.Employee>(!!0)
			  IL_0007:  stloc.0
			  IL_0008:  ldloc.0
			  IL_0009:  ldarg.2
			  IL_000a:  callvirt   instance void MMDB.Test.Employee::set_ID(int32)
			  IL_000f:  nop
			  IL_0010:  ldloc.0
			  IL_0011:  stloc.1
			  IL_0012:  br.s       IL_0014
			  IL_0014:  ldloc.1
			  IL_0015:  ret
			} // end of method Program::Update*/


		//}
	}
}
