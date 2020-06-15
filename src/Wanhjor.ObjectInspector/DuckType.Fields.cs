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
            var isPublicInstance = instanceType.IsPublic || instanceType.IsNestedPublic;
            var returnType = field.FieldType;

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

            if (isPublicInstance && field.IsPublic)
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
                    il.Emit(OpCodes.Ldnull);
                else
                    ILHelpers.LoadInstance(il, instanceField, instanceType);

                returnType = typeof(object);
                var dynParameters = new[] {typeof(object)};
                var dynMethod = new DynamicMethod($"_getField+{field.DeclaringType!.Name}.{field.Name}", returnType, dynParameters, typeof(EmitAccessors).Module);
                EmitAccessors.CreateGetAccessor(dynMethod.GetILGenerator(), field);
                var handle = GetRuntimeHandle(dynMethod);

                il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, returnType, dynParameters, null);
                DynamicMethods.Add(dynMethod);
            }

            if (innerDuck)
                il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
            else if (returnType != iProperty.PropertyType)
                ILHelpers.TypeConversion(il, returnType, iProperty.PropertyType);

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
                new[] {iProperty.PropertyType});

            var il = method.GetILGenerator();
            var isPublicInstance = instanceType.IsPublic || instanceType.IsNestedPublic;

            if ((field.Attributes & FieldAttributes.InitOnly) != 0)
            {
                il.Emit(OpCodes.Newobj, typeof(DuckTypeFieldIsReadonlyException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
                return method;
            }

            // Load instance
            if (!isPublicInstance || !field.IsPublic)
            {
                if (field.IsStatic)
                    il.Emit(OpCodes.Ldnull);
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, instanceField);
                }
            } else if (!field.IsStatic)
                ILHelpers.LoadInstance(il, instanceField, instanceType); 

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

            // Call method
            if (isPublicInstance && field.IsPublic)
            {
                var fieldRootType = Util.GetRootType(field.FieldType);
                var iPropRootType = Util.GetRootType(iProperty.PropertyType);
                ILHelpers.TypeConversion(il, iPropRootType, fieldRootType);
                    
                il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
            }
            else
            {
                var iPropRootType = Util.GetRootType(iProperty.PropertyType);
                ILHelpers.TypeConversion(il, iPropRootType, typeof(object));

                var dynParameters = new[] {typeof(object), typeof(object)};
                var dynMethod = new DynamicMethod($"_setField+{field.DeclaringType!.Name}.{field.Name}", typeof(void), dynParameters, typeof(EmitAccessors).Module);
                EmitAccessors.CreateSetAccessor(dynMethod.GetILGenerator(), field);
                var handle = GetRuntimeHandle(dynMethod);

                il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, dynMethod.CallingConvention, typeof(void), dynParameters, null);
                DynamicMethods.Add(dynMethod);
            }

            il.Emit(OpCodes.Ret);
            return method;
        }
    }
}