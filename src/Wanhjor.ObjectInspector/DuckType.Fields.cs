using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wanhjor.ObjectInspector
{
    public partial class DuckType
    {
        private static MethodBuilder GetFieldGetMethod(Type instanceType, TypeBuilder typeBuilder,
            PropertyInfo iProperty, FieldInfo field, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod("get_" + iProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                iProperty.PropertyType, Type.EmptyTypes);

            var il = method.GetILGenerator();
            
            var innerDuck = false;
            var iPropTypeInterface = iProperty.PropertyType;
            if (iPropTypeInterface.IsGenericType)
                iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
            if (iProperty.PropertyType != field.FieldType && iProperty.PropertyType.IsInterface && field.FieldType.GetInterface(iPropTypeInterface.FullName) == null)
            {
                if (field.IsStatic)
                {
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + iProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerField);
                }
                else
                {
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + iProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                }
                il.Emit(OpCodes.Ldtoken, iProperty.PropertyType);
                il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                innerDuck = true;
            }

            if (instanceType.IsPublic || instanceType.IsNestedPublic)
            {
                if (field.IsPublic)
                {
                    if (field.IsStatic)
                    {
                        il.Emit(OpCodes.Ldsfld, field);
                    }
                    else
                    {
                        ILHelpers.LoadInstance(il, instanceField, instanceType);
                        il.Emit(OpCodes.Ldfld, field);
                    }
                }
                else
                {
                    if (field.IsStatic)
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                    else
                    {
                        ILHelpers.LoadInstance(il, instanceField, instanceType);
                    }

                    var getMethod = new DynamicMethod($"GetField+{field.DeclaringType!.Name}.{field.Name}", typeof(object), new[] {typeof(object)}, typeof(EmitAccessors).Module);
                    EmitAccessors.CreateGetAccessor(getMethod.GetILGenerator(), field);
                    var handle = GetRuntimeHandle(getMethod);

                    il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, getMethod.CallingConvention,
                        getMethod.ReturnType,
                        getMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                    DynamicMethods.Add(getMethod);
                }
                
                if (innerDuck)
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                else if (field.FieldType != iProperty.PropertyType)
                    ILHelpers.TypeConversion(il, field.FieldType, iProperty.PropertyType);

            }
            else
            {
                // We can't access to a non public instance using IL, So we need to get the field value using a dynamic fetcher
                var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dFetcher" + iProperty.Name, typeBuilder), tuple =>
                    tuple.Item2.DefineField(tuple.Item1, typeof(DynamicFetcher), FieldAttributes.Private));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldflda, innerField);
                il.Emit(OpCodes.Ldstr, field.Name);
                if (!field.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, instanceField);
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                }
                il.EmitCall(OpCodes.Call, FetchMethodInfo, null);
                
                if (innerDuck)
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                else if (iProperty.PropertyType != typeof(object))
                    ILHelpers.TypeConversion(il, typeof(object), iProperty.PropertyType);
            }

            il.Emit(OpCodes.Ret);
            return method;
        }

        private static MethodBuilder GetFieldSetMethod(Type instanceType, TypeBuilder typeBuilder,
            PropertyInfo iProperty, FieldInfo field, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod("set_" + iProperty.Name, 
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                typeof(void), 
                new[]{ iProperty.PropertyType });

            var il = method.GetILGenerator();

            if ((field.Attributes & FieldAttributes.InitOnly) != 0)
            {
                il.Emit(OpCodes.Newobj, typeof(DuckTypeFieldIsReadonlyException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }
            else
            {
                if (instanceType.IsPublic || instanceType.IsNestedPublic)
                {
                    // Load instance
                    if (!field.IsStatic)
                        ILHelpers.LoadInstance(il, instanceField, instanceType);
                }
                else
                {
                    // We can't access to a non public instance using IL, So we need to set the field value using a dynamic fetcher
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dFetcher" + iProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DynamicFetcher), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                    il.Emit(OpCodes.Ldstr, field.Name);
                    if (!field.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, instanceField);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                }

                // Check if a duck type object
                var iPropTypeInterface = iProperty.PropertyType;
                if (iPropTypeInterface.IsGenericType)
                    iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
                if (iProperty.PropertyType != field.FieldType && iProperty.PropertyType.IsInterface && field.FieldType.GetInterface(iPropTypeInterface.FullName) == null)
                {
                    if (field.IsStatic)
                    {
                        var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                        il.Emit(OpCodes.Ldsflda, innerField);
                    }
                    else
                    {
                        var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldflda, innerField);
                    }
                    // Load value
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, typeof(DuckType));
                    il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
                }
                else
                {
                    // Load value
                    il.Emit(OpCodes.Ldarg_1);
                }

                if (instanceType.IsPublic || instanceType.IsNestedPublic)
                {
                    var fieldRootType = Util.GetRootType(field.FieldType);
                    var iPropRootType = Util.GetRootType(iProperty.PropertyType);
                    ILHelpers.TypeConversion(il, iPropRootType, fieldRootType);
                    
                    // Call method
                    if (field.IsPublic)
                    {
                        il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
                    }
                    else
                    {
                        var setMethod = new DynamicMethod($"SetField+{field.DeclaringType!.Name}.{field.Name}", typeof(void), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
                        EmitAccessors.CreateSetAccessor(setMethod.GetILGenerator(), field);
                        var handle = GetRuntimeHandle(setMethod);

                        il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                        il.Emit(OpCodes.Conv_I);
                        il.EmitCalli(OpCodes.Calli, setMethod.CallingConvention,
                            setMethod.ReturnType,
                            setMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                            null);
                        DynamicMethods.Add(setMethod);
                    }
                }
                else
                {
                    var iPropRootType = Util.GetRootType(iProperty.PropertyType);
                    ILHelpers.TypeConversion(il, iPropRootType, typeof(object));
                    
                    // We can't access to a non public instance using IL, So we need to set the field value using a dynamic fetcher
                    il.EmitCall(OpCodes.Call, ShoveMethodInfo, null);
                }
            }
            il.Emit(OpCodes.Ret);

            return method;
        }

    }
}