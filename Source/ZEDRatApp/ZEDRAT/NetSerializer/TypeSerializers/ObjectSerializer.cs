using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ZEDRatApp.ZEDRAT.NetSerializer.TypeSerializers
{
	internal class ObjectSerializer : IDynamicTypeSerializer, ITypeSerializer
	{
		public bool Handles(Type type)
		{
			return type == typeof(object);
		}

		public IEnumerable<Type> GetSubtypes(Type type)
		{
			return new Type[0];
		}

		public void GenerateWriterMethod(Type obtype, CodeGenContext ctx, ILGenerator il)
		{
			MethodInfo method = typeof(Serializer).GetMethod("GetTypeID", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[1] { typeof(object) }, null);
			IDictionary<Type, TypeData> typeMap = ctx.TypeMap;
			LocalBuilder local = il.DeclareLocal(typeof(ushort));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Call, method);
			il.Emit(OpCodes.Stloc_S, local);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloc_S, local);
			il.Emit(OpCodes.Call, ctx.GetWriterMethodInfo(typeof(ushort)));
			Label[] array = new Label[typeMap.Count + 1];
			array[0] = il.DefineLabel();
			foreach (KeyValuePair<Type, TypeData> item in typeMap)
			{
				array[item.Value.TypeID] = il.DefineLabel();
			}
			il.Emit(OpCodes.Ldloc_S, local);
			il.Emit(OpCodes.Switch, array);
			il.Emit(OpCodes.Newobj, Helpers.ExceptionCtorInfo);
			il.Emit(OpCodes.Throw);
			il.MarkLabel(array[0]);
			il.Emit(OpCodes.Ret);
			foreach (KeyValuePair<Type, TypeData> item2 in typeMap)
			{
				Type key = item2.Key;
				TypeData value = item2.Value;
				il.MarkLabel(array[value.TypeID]);
				if (value.NeedsInstanceParameter)
				{
					il.Emit(OpCodes.Ldarg_0);
				}
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldarg_2);
				il.Emit(key.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, key);
				il.Emit(OpCodes.Tailcall);
				il.Emit(OpCodes.Call, value.WriterMethodInfo);
				il.Emit(OpCodes.Ret);
			}
		}

		public void GenerateReaderMethod(Type obtype, CodeGenContext ctx, ILGenerator il)
		{
			IDictionary<Type, TypeData> typeMap = ctx.TypeMap;
			LocalBuilder local = il.DeclareLocal(typeof(ushort));
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldloca_S, local);
			il.Emit(OpCodes.Call, ctx.GetReaderMethodInfo(typeof(ushort)));
			Label[] array = new Label[typeMap.Count + 1];
			array[0] = il.DefineLabel();
			foreach (KeyValuePair<Type, TypeData> item in typeMap)
			{
				array[item.Value.TypeID] = il.DefineLabel();
			}
			il.Emit(OpCodes.Ldloc_S, local);
			il.Emit(OpCodes.Switch, array);
			il.Emit(OpCodes.Newobj, Helpers.ExceptionCtorInfo);
			il.Emit(OpCodes.Throw);
			il.MarkLabel(array[0]);
			il.Emit(OpCodes.Ldarg_2);
			il.Emit(OpCodes.Ldnull);
			il.Emit(OpCodes.Stind_Ref);
			il.Emit(OpCodes.Ret);
			foreach (KeyValuePair<Type, TypeData> item2 in typeMap)
			{
				Type key = item2.Key;
				TypeData value = item2.Value;
				il.MarkLabel(array[value.TypeID]);
				LocalBuilder localBuilder = il.DeclareLocal(key);
				if (value.NeedsInstanceParameter)
				{
					il.Emit(OpCodes.Ldarg_0);
				}
				il.Emit(OpCodes.Ldarg_1);
				if (localBuilder.LocalIndex < 256)
				{
					il.Emit(OpCodes.Ldloca_S, localBuilder);
				}
				else
				{
					il.Emit(OpCodes.Ldloca, localBuilder);
				}
				il.Emit(OpCodes.Call, value.ReaderMethodInfo);
				il.Emit(OpCodes.Ldarg_2);
				if (localBuilder.LocalIndex < 256)
				{
					il.Emit(OpCodes.Ldloc_S, localBuilder);
				}
				else
				{
					il.Emit(OpCodes.Ldloc, localBuilder);
				}
				if (key.IsValueType)
				{
					il.Emit(OpCodes.Box, key);
				}
				il.Emit(OpCodes.Stind_Ref);
				il.Emit(OpCodes.Ret);
			}
		}
	}
}
