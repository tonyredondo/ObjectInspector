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
            var il = getPropMethod.GetILGenerator();
            if (property.GetMethod.IsStatic)
            {
                il.EmitCall(OpCodes.Call, property.GetMethod, null);
                if (property.DeclaringType.IsValueType)
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
                    il.EmitCall(OpCodes.Callvirt, property.GetMethod, null);
                    il.Emit(OpCodes.Box, property.PropertyType);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, property.DeclaringType);
                    il.EmitCall(OpCodes.Callvirt, property.GetMethod, null);
                }
                il.Emit(OpCodes.Ret);
            }

            return (Func<object, object>) getPropMethod.CreateDelegate(typeof(Func<object, object>));
        }
    }
}