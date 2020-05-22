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
        /// <summary>
        /// Build a get accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object> BuildGetAccessor(PropertyInfo property)
        {
            var getMethod = new DynamicMethod($"GetProp+{property.DeclaringType.Name}.{property.Name}", typeof(object), new[] {typeof(object)}, typeof(EmitAccessors).Module);
            CreateGetAccessor(getMethod.GetILGenerator(), property);
            return (Func<object, object>) getMethod.CreateDelegate(typeof(Func<object, object>));
        }

        /// <summary>
        /// Creates the IL code for a get property
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature:
        /// object (object instance);
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="property">Property info</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateGetAccessor(ILGenerator il, PropertyInfo property)
        {
            if (!property.CanRead)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException));
                il.Emit(OpCodes.Throw);
            }
            
            if (property.GetMethod.IsStatic)
            {
                il.EmitCall(OpCodes.Call, property.GetMethod, null);
                if (property.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, property.PropertyType);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (property.DeclaringType.IsValueType)
                {
                    il.Emit( OpCodes.Unbox_Any, property.DeclaringType);
                    il.Emit( OpCodes.Stloc_0);
                    il.Emit( OpCodes.Ldloca_S, 0);
                }
                else if (property.DeclaringType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, property.DeclaringType);
                }
                
                il.EmitCall(OpCodes.Callvirt, property.GetMethod, null);
                if (property.PropertyType.IsValueType)
                    il.Emit(OpCodes.Box, property.PropertyType);
                il.Emit(OpCodes.Ret);
            }
        }

        /// <summary>
        /// Build a set accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<object, object> BuildSetAccessor(PropertyInfo property)
        {
            var setMethod = new DynamicMethod($"SetProp+{property.DeclaringType.Name}.{property.Name}", typeof(void), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
            CreateSetAccessor(setMethod.GetILGenerator(), property);
            return (Action<object, object>) setMethod.CreateDelegate(typeof(Action<object, object>));
        }
        
        /// <summary>
        /// Creates the IL code for a set property
        /// </summary>
        /// <remarks>
        /// Methods should accomplish the following signature when the declaring type is a class:
        /// void (object instance, object value);
        /// Methods should accomplish the following signature when the declaring type is a value:
        /// void (ref object instance, object value);
        /// </remarks>
        /// <param name="il">Il Generator</param>
        /// <param name="property">Property info</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateSetAccessor(ILGenerator il, PropertyInfo property)
        {
            if (!property.CanWrite)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException));
                il.Emit(OpCodes.Throw);
            }
            
            if (property.SetMethod.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_1);
                if (property.PropertyType.IsValueType)
                    il.Emit( OpCodes.Unbox_Any, property.PropertyType);
                else if (property.PropertyType != typeof(object))
                    il.Emit(OpCodes.Castclass, property.PropertyType);
                il.EmitCall(OpCodes.Call, property.SetMethod, null);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (property.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Ldind_Ref);
                    il.Emit( OpCodes.Unbox_Any, property.DeclaringType);
                    il.Emit( OpCodes.Stloc_0);
                    il.Emit( OpCodes.Ldloca_S, 0);
                }
                else if (property.DeclaringType != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, property.DeclaringType);
                }
                il.Emit(OpCodes.Ldarg_1);
                if (property.PropertyType.IsValueType)
                    il.Emit( OpCodes.Unbox_Any, property.PropertyType);
                else if (property.PropertyType != typeof(object))
                    il.Emit(OpCodes.Castclass, property.PropertyType);
                il.EmitCall(OpCodes.Callvirt, property.SetMethod, null);
                if (property.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldloc_0);
                    il.Emit(OpCodes.Box, property.DeclaringType);
                    il.Emit(OpCodes.Stind_Ref);
                }
                il.Emit(OpCodes.Ret);
            }
        }
        
        
        
        /// <summary>
        /// Build a get accessor from a field info
        /// </summary>
        /// <param name="field">Field info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object> BuildGetAccessor(FieldInfo field)
        {
            var getMethod = new DynamicMethod($"GetField+{field.DeclaringType.Name}.{field.Name}", typeof(object), new[] {typeof(object)}, typeof(EmitAccessors).Module);
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
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType.IsValueType)
                    il.Emit( OpCodes.Unbox, field.DeclaringType);
                else if (field.DeclaringType != typeof(object))
                    il.Emit(OpCodes.Castclass, field.DeclaringType);
                il.Emit(OpCodes.Ldfld, field);
                if (field.FieldType.IsValueType)
                    il.Emit(OpCodes.Box, field.FieldType);
                il.Emit(OpCodes.Ret);
            }
        }
        
        /// <summary>
        /// Build a set accessor from a field info
        /// </summary>
        /// <param name="field">Field info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<object, object> BuildSetAccessor(FieldInfo field)
        {
            var setMethod = new DynamicMethod($"SetField+{field.DeclaringType.Name}.{field.Name}", typeof(void), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
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
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException));
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
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                if (field.DeclaringType.IsValueType)
                {
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
                il.Emit(OpCodes.Ret);
            }
        }

        
        
        /// <summary>
        /// Create an accessor delegate for a MethodInfo
        /// </summary>
        /// <param name="method">Method info instance</param>
        /// <returns>Accessor delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object[], object> BuildMethodAccessor(MethodInfo method)
        {
            var lstParams = new List<string>();
            var gParams = method.GetParameters();
            foreach (var p in gParams)
                lstParams.Add(p.ParameterType.Name);
            var callMethod = new DynamicMethod($"Call+{method.DeclaringType.Name}.{method.Name}+{string.Join("_", lstParams)}", typeof(void), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
            CreateMethodAccessor(callMethod.GetILGenerator(), method);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateMethodAccessor(ILGenerator il, MethodInfo method)
        {
            
        }
    }
}