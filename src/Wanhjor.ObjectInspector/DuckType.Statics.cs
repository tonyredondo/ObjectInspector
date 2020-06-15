using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace Wanhjor.ObjectInspector
{
    public partial class DuckType
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod("GetTypeFromHandle")!;
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo DuckTypeCreate = typeof(DuckType).GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Type), typeof(object) }, null)!;
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly ConcurrentDictionary<VTuple<Type,Type>, Type> DuckTypeCache = new ConcurrentDictionary<VTuple<Type,Type>, Type>();
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly ConcurrentBag<DynamicMethod> DynamicMethods = new ConcurrentBag<DynamicMethod>();
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo GetInnerDuckTypeMethodInfo = typeof(DuckType).GetMethod("GetInnerDuckType", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo SetInnerDuckTypeMethodInfo = typeof(DuckType).GetMethod("SetInnerDuckType", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo InvokeMethodInfo = typeof(DuckType).GetMethod("Invoke", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly ConcurrentDictionary<VTuple<string, TypeBuilder>, FieldInfo> DynamicFields = new ConcurrentDictionary<VTuple<string, TypeBuilder>, FieldInfo>();
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        private static Func<DynamicMethod, RuntimeMethodHandle>? _dynamicGetMethodDescriptor;

        internal static RuntimeMethodHandle GetRuntimeHandle(DynamicMethod dynamicMethod)
        {
            _dynamicGetMethodDescriptor ??= (Func<DynamicMethod, RuntimeMethodHandle>) typeof(DynamicMethod)
                .GetMethod("GetMethodDescriptor", BindingFlags.NonPublic | BindingFlags.Instance)
                .CreateDelegate(typeof(Func<DynamicMethod, RuntimeMethodHandle>));
            return _dynamicGetMethodDescriptor(dynamicMethod);
        }
    }
}