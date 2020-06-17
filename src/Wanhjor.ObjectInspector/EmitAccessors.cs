using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Accessors using IL
    /// </summary>
    public static class EmitAccessors
    {
        private static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod("GetTypeFromHandle");
        private static readonly MethodInfo EnumToObjectMethodInfo = typeof(Enum).GetMethod("ToObject", new[] { typeof(Type), typeof(object) });
        private static readonly MethodInfo ConvertTypeMethodInfo = typeof(Util).GetMethod("ConvertType");
        
        /// <summary>
        /// Build a get accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object> BuildGetAccessor(PropertyInfo property)
        {
            var method = new DynamicMethod($"GetProp+{property.DeclaringType!.Name}.{property.Name}", typeof(object), new[] {typeof(object)}, typeof(EmitAccessors).Module);
            CreateGetAccessor(method.GetILGenerator(), property, typeof(object), typeof(object));
            return (Func<object, object>) method.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// Creates the IL code for a get property
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature:
        /// [returnType] ([instanceType] instance);
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="property">Property info</param>
        /// <param name="instanceType">Instance type</param>
        /// <param name="returnType">Return type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateGetAccessor(ILGenerator il, PropertyInfo property, Type instanceType, Type returnType)
        {
            if (!property.CanRead)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }
            
            if (property.GetMethod.IsStatic)
            {
                il.EmitCall(OpCodes.Call, property.GetMethod, null);
            }
            else
            {
                ILHelpers.LoadInstanceArgument(il, instanceType, property.DeclaringType);
                il.EmitCall(OpCodes.Callvirt, property.GetMethod, null);
            }
            ILHelpers.TypeConversion(il, property.PropertyType, returnType);
            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Build a set accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<object, object> BuildSetAccessor(PropertyInfo property)
        {
            var setMethod = new DynamicMethod($"SetProp+{property.DeclaringType!.Name}.{property.Name}", typeof(void), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
            CreateSetAccessor(setMethod.GetILGenerator(), property, typeof(object), typeof(object));
            return (Action<object, object>) setMethod.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        /// Creates the IL code for a set property
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature when the declaring type is a class:
        /// void ([instanceType] instance, [valueType] value);
        /// Methods should accomplish the following signature when the declaring type is a value:
        /// void (ref [instanceType] instance, [valueType] value);
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="property">Property info</param>
        /// <param name="instanceType">Instance type</param>
        /// <param name="valueType">Value type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateSetAccessor(ILGenerator il, PropertyInfo property, Type instanceType, Type valueType)
        {
            if (!property.CanWrite)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }
            
            if (property.SetMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_1);
                ILHelpers.TypeConversion(il, valueType, property.PropertyType);
                il.EmitCall(OpCodes.Call, property.SetMethod, null);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (property.DeclaringType!.IsValueType)
                {
                    il.DeclareLocal(property.DeclaringType);
                    il.Emit(OpCodes.Ldind_Ref);
                    il.Emit( OpCodes.Unbox_Any, property.DeclaringType);
                    il.Emit( OpCodes.Stloc_0);
                    il.Emit( OpCodes.Ldloca_S, 0);
                }
                else if (property.DeclaringType != instanceType)
                {
                    il.Emit(OpCodes.Castclass, property.DeclaringType);
                }
                il.Emit(OpCodes.Ldarg_1);
                ILHelpers.TypeConversion(il, valueType, property.PropertyType);
                il.EmitCall(OpCodes.Callvirt, property.SetMethod, null);
                if (property.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Box, property.DeclaringType);
                    il.Emit(OpCodes.Stind_Ref);
                }
            }
            il.Emit(OpCodes.Ret);
        }
        
        
        
        /// <summary>
        /// Build a get accessor from a field info
        /// </summary>
        /// <param name="field">Field info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object> BuildGetAccessor(FieldInfo field)
        {
            var getMethod = new DynamicMethod($"GetField+{field.DeclaringType!.Name}.{field.Name}", typeof(object), new[] {typeof(object)}, typeof(EmitAccessors).Module);
            CreateGetAccessor(getMethod.GetILGenerator(), field);
            return (Func<object, object>) getMethod.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// Creates the IL code for a get of a field
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature:
        /// object (object instance);
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="field">Field info</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateGetAccessor(ILGenerator il, FieldInfo field)
        {
            if (field.IsStatic)
            {
                il.Emit(OpCodes.Ldsfld, field);
                if (field.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, field.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType!.IsValueType)
                    il.Emit( OpCodes.Unbox, field.DeclaringType);
                else if (field.DeclaringType != typeof(object))
                    il.Emit(OpCodes.Castclass, field.DeclaringType);
                il.Emit(OpCodes.Ldfld, field);
                if (field.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, field.FieldType);
            }
            il.Emit(OpCodes.Ret);
        }
        
        /// <summary>
        /// Build a set accessor from a field info
        /// </summary>
        /// <param name="field">Field info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<object, object> BuildSetAccessor(FieldInfo field)
        {
            var setMethod = new DynamicMethod($"SetField+{field.DeclaringType!.Name}.{field.Name}", typeof(void), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
            CreateSetAccessor(setMethod.GetILGenerator(), field);
            return (Action<object, object>) setMethod.CreateDelegate(typeof(Action<object, object>));
        }
        
        /// <summary>
        /// Creates the IL code for a set a field
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature when the declaring type is a class:
        /// void (object instance, object value);
        /// Methods should accomplish the following signature when the declaring type is a value:
        /// void (ref object instance, object value);
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="field">Field info</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateSetAccessor(ILGenerator il, FieldInfo field)
        {
            if ((field.Attributes & FieldAttributes.InitOnly) != 0)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }
            
            if (field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_1);
                if (field.FieldType.IsValueType)
                    il.Emit( OpCodes.Unbox_Any, field.FieldType);
                else if (field.FieldType != typeof(object))
                    il.Emit(OpCodes.Castclass, field.FieldType);
                il.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType!.IsValueType)
                {
                    il.DeclareLocal(field.DeclaringType);
                    il.Emit(OpCodes.Ldind_Ref);
                    il.Emit( OpCodes.Unbox_Any, field.DeclaringType);
                    il.Emit( OpCodes.Stloc_0);
                    il.Emit( OpCodes.Ldloca_S, 0);
                }
                else if (field.DeclaringType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, field.DeclaringType);
                }
                il.Emit(OpCodes.Ldarg_1);
                if (field.FieldType.IsValueType)
                    il.Emit( OpCodes.Unbox_Any, field.FieldType);
                else if (field.FieldType != typeof(object))
                    il.Emit(OpCodes.Castclass, field.FieldType);
                il.Emit(OpCodes.Stfld, field);
                if (field.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Box, field.DeclaringType);
                    il.Emit(OpCodes.Stind_Ref);
                }
            }
            il.Emit(OpCodes.Ret);
        }

        
        
        /// <summary>
        /// Create an accessor delegate for a MethodInfo
        /// </summary>
        /// <param name="method">Method info instance</param>
        /// <returns>Accessor delegate</returns>
        /// <param name="strict">Creates an strict accessor without basic conversion for IConvertibles</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object[], object> BuildMethodAccessor(MethodInfo method, bool strict)
        {
            var lstParams = new List<string>();
            var gParams = method.GetParameters();
            foreach (var p in gParams)
                lstParams.Add(p.ParameterType.Name);
            var callMethod = new DynamicMethod($"Call+{method.DeclaringType!.Name}.{method.Name}+{string.Join("_", lstParams)}", typeof(object), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
            CreateMethodAccessor(callMethod.GetILGenerator(), method, strict);
            return (Func<object, object[], object>) callMethod.CreateDelegate(typeof(Func<object, object[], object>));
        }
        /// <summary>
        /// Creates the IL code for calling a method
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature:
        /// object (object instance, object[] args)
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="method">Method info</param>
        /// <param name="strict">Creates an strict accessor without basic conversion for IConvertibles</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateMethodAccessor(ILGenerator il, MethodInfo method, bool strict)
        {
            // Prepare instance
            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                if (method.DeclaringType!.IsValueType)
                {
                    il.Emit( OpCodes.Unbox_Any, method.DeclaringType);
                    il.Emit( OpCodes.Stloc_0);
                    il.Emit( OpCodes.Ldloca_S, 0);
                } 
                else if (method.DeclaringType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, method.DeclaringType);
                }
            }
            
            // Prepare arguments
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var pType = parameters[i].ParameterType;
                var rType = Util.GetRootType(pType);
                var callEnum = false;
                if (rType.IsEnum)
                {
                    il.Emit(OpCodes.Ldtoken, rType);
                    il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    callEnum = true;
                }
                    
                il.Emit(OpCodes.Ldarg_1);
                ILHelpers.WriteIlIntValue(il, i);
                il.Emit(OpCodes.Ldelem_Ref);

                if (callEnum)
                {
                    il.EmitCall(OpCodes.Call, EnumToObjectMethodInfo, null);
                } 
                else if (!strict && pType != typeof(object))
                {
                    il.Emit(OpCodes.Ldtoken, rType);
                    il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    il.EmitCall(OpCodes.Call, ConvertTypeMethodInfo, null);
                }
                    
                if (pType.IsValueType)
                    il.Emit( OpCodes.Unbox_Any, pType);
                else if (pType != typeof(object))
                    il.Emit(OpCodes.Castclass, pType);
            }

            // Call method
            il.EmitCall(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method, null);

            // Prepare return
            if (method.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else if (method.ReturnType.IsValueType)
                il.Emit(OpCodes.Box, method.ReturnType);
            
            il.Emit(OpCodes.Ret);
        }
    }
}