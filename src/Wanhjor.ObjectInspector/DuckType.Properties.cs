using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wanhjor.ObjectInspector
{
    public partial class DuckType
    {
        private static Type[] GetPropertyParameterTypes(PropertyInfo property, bool includePropertyType)
        {
            var idxParams = property.GetIndexParameters();
            if (idxParams.Length == 0) return includePropertyType ? new[] {property.PropertyType} : Type.EmptyTypes;

            var parameterTypes = new Type[includePropertyType ? idxParams.Length + 1 : idxParams.Length];
            for (var i = 0; i < idxParams.Length; i++)
                parameterTypes[i] = idxParams[i].ParameterType;
            if (includePropertyType)
                parameterTypes[idxParams.Length] = property.PropertyType;
            return parameterTypes;
        }

        private static MethodBuilder GetPropertyGetMethod(Type instanceType, TypeBuilder typeBuilder,
            PropertyInfo iProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            var parameterTypes = GetPropertyParameterTypes(iProperty, false);
            var method = typeBuilder.DefineMethod("get_" + iProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig | MethodAttributes.Virtual,
                iProperty.PropertyType, parameterTypes);
            var il = method.GetILGenerator();

            if (!prop.CanRead)
            {
                il.Emit(OpCodes.Newobj, typeof(DuckTypePropertyCantBeReadException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
                return method;
            }

            var propMethod = prop.GetMethod;

            // Check if an inner duck type is needed
            var innerDuck = false;
            var iPropTypeInterface = iProperty.PropertyType;
            if (iPropTypeInterface.IsGenericType)
                iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
            if (iProperty.PropertyType != prop.PropertyType && parameterTypes.Length == 0 && iProperty.PropertyType.IsInterface && prop.PropertyType.GetInterface(iPropTypeInterface.FullName) == null)
            {
                if (propMethod.IsStatic)
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

            // Load the instance
            if (!propMethod.IsStatic)
                ILHelpers.LoadInstance(il, instanceField, instanceType);

            if (instanceType.IsPublic || instanceType.IsNestedPublic)
            {
                // If we have index parameters we need to pass it
                if (parameterTypes.Length > 0)
                {
                    var propIdxParams = prop.GetIndexParameters();
                    for (var i = 0; i < parameterTypes.Length; i++)
                    {
                        ILHelpers.WriteLoadArgument(i, il, propMethod.IsStatic);
                        var iPType = Util.GetRootType(parameterTypes[i]);
                        var pType = Util.GetRootType(propIdxParams[i].ParameterType);
                        ILHelpers.TypeConversion(il, iPType, pType);
                    }
                }

                // Method call
                if (propMethod.IsPublic)
                {
                    il.EmitCall(propMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propMethod, null);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long) propMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, propMethod.CallingConvention,
                        propMethod.ReturnType,
                        propMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                }

                // Handle return value
                if (innerDuck)
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                else if (prop.PropertyType != iProperty.PropertyType)
                    ILHelpers.TypeConversion(il, prop.PropertyType, iProperty.PropertyType);
            }
            else
            {
                if (propMethod.IsStatic)
                    il.Emit(OpCodes.Ldnull);

                var dynParameters = new[] {typeof(object)};
                var dynMethod = new DynamicMethod("getDyn_" + prop.Name, typeof(object), dynParameters, typeof(EmitAccessors).Module);
                EmitAccessors.CreateGetAccessor(dynMethod.GetILGenerator(), prop);
                var handle = GetRuntimeHandle(dynMethod);

                il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention,
                    dynMethod.ReturnType,
                    dynParameters.ToArray(),
                    null);
                DynamicMethods.Add(dynMethod);

                // Handle return value
                if (innerDuck)
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                else if (typeof(object) != iProperty.PropertyType)
                    ILHelpers.TypeConversion(il, typeof(object), iProperty.PropertyType);
            }

            il.Emit(OpCodes.Ret);
            return method;
        }

        private static MethodBuilder GetPropertySetMethod(Type instanceType, TypeBuilder typeBuilder,
            PropertyInfo iProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            var parameterTypes = GetPropertyParameterTypes(iProperty, true);
            var method = typeBuilder.DefineMethod("set_" + iProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName |
                MethodAttributes.HideBySig | MethodAttributes.Virtual,
                typeof(void),
                parameterTypes);

            var il = method.GetILGenerator();

            if (!prop.CanWrite)
            {
                il.Emit(OpCodes.Newobj, typeof(DuckTypePropertyCantBeWrittenException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
                return method;
            }

            var propMethod = prop.SetMethod;

            if (instanceType.IsPublic || instanceType.IsNestedPublic)
            {
                // Load instance
                if (!propMethod.IsStatic)
                    ILHelpers.LoadInstance(il, instanceField, instanceType);

                // Check if a duck type object
                var iPropTypeInterface = iProperty.PropertyType;
                if (iPropTypeInterface.IsGenericType)
                    iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
                if (iProperty.PropertyType != prop.PropertyType && parameterTypes.Length == 1 && iProperty.PropertyType.IsInterface && prop.PropertyType.GetInterface(iPropTypeInterface.FullName) == null)
                {
                    if (propMethod.IsStatic)
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
                    // Load values
                    // If we have index parameters we need to pass it
                    var propTypes = GetPropertyParameterTypes(prop, true);
                    for (var i = 0; i < parameterTypes.Length; i++)
                    {
                        ILHelpers.WriteLoadArgument(i, il, propMethod.IsStatic);
                        var propRootType = Util.GetRootType(propTypes[i]);
                        var iPropRootType = Util.GetRootType(parameterTypes[i]);
                        ILHelpers.TypeConversion(il, iPropRootType, propRootType);
                    }
                }

                // Call method
                if (propMethod.IsPublic)
                {
                    il.EmitCall(propMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propMethod, null);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long) propMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, propMethod.CallingConvention,
                        propMethod.ReturnType,
                        propMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                }
            }
            else
            {
                // We can't access to a non public instance using IL, So we need to set the property value using a dynamic fetcher

                var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dFetcher" + iProperty.Name, typeBuilder), tuple =>
                    tuple.Item2.DefineField(tuple.Item1, typeof(DynamicFetcher), FieldAttributes.Private));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldflda, innerField);
                il.Emit(OpCodes.Ldstr, prop.Name);
                if (!propMethod.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, instanceField);
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                }

                // Check if a duck type object
                var iPropTypeInterface = iProperty.PropertyType;
                if (iPropTypeInterface.IsGenericType)
                    iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
                if (iProperty.PropertyType != prop.PropertyType && parameterTypes.Length == 1 && iProperty.PropertyType.IsInterface && prop.PropertyType.GetInterface(iPropTypeInterface.FullName) == null)
                {
                    if (propMethod.IsStatic)
                    {
                        var iField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                        il.Emit(OpCodes.Ldsflda, iField);
                    }
                    else
                    {
                        var iField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldflda, iField);
                    }

                    // Load value
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, typeof(DuckType));
                    il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
                }
                else
                {
                    // Load values
                    for (var i = 0; i < parameterTypes.Length; i++)
                    {
                        ILHelpers.WriteLoadArgument(i, il, propMethod.IsStatic);
                        var iPropRootType = Util.GetRootType(parameterTypes[i]);
                        ILHelpers.TypeConversion(il, iPropRootType, typeof(object));
                    }
                }

                // We can't access to a non public instance using IL, So we need to set the property value using a dynamic fetcher
                il.EmitCall(OpCodes.Call, ShoveMethodInfo, null);
            }

            il.Emit(OpCodes.Ret);

            return method;
        }
    }
}