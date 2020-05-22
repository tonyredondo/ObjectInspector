using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Accessors using IL
    /// </summary>
    internal static class EmitAccessors
    {
        public static Func<object, object> BuildGetAccessor(PropertyInfo property)
        {
            var getPropMethod = new DynamicMethod($"GetProp+{property.DeclaringType.Name}.{property.Name}", typeof(object), new[] {typeof(object)}, typeof(EmitAccessors).Module);
            CreateGetAccessor(getPropMethod.GetILGenerator(), property);
            return (Func<object, object>) getPropMethod.CreateDelegate(typeof(Func<object, object>));
        }

        public static Action<object, object> BuildSetAccessor(PropertyInfo property)
        {
            var setPropMethod = new DynamicMethod($"SetProp+{property.DeclaringType.Name}.{property.Name}", typeof(void), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
            CreateSetAccessor(setPropMethod.GetILGenerator(), property);
            return (Action<object, object>) setPropMethod.CreateDelegate(typeof(Action<object, object>));
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
                    il.Emit(OpCodes.Box, property.PropertyType);
                    il.Emit(OpCodes.Stind_Ref);
                }
                il.Emit(OpCodes.Ret);
            }
        }
    }
}