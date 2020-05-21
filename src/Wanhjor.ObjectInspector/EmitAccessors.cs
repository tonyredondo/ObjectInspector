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
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(property.DeclaringType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, property.DeclaringType);
                il.EmitCall(OpCodes.Call, property.GetMethod, null);
                il.Emit(OpCodes.Ret);
            }

            return (Func<object, object>) getPropMethod.CreateDelegate(typeof(Func<object, object>));
        }
    }
}