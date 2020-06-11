using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Wanhjor.ObjectInspector
{
    public partial class DuckType
    {
        /// <summary>
        /// Create duck type proxy from an interface
        /// </summary>
        /// <param name="instance">Instance object</param>
        /// <typeparam name="T">Interface type</typeparam>
        /// <returns>Duck type proxy</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Create<T>(object instance)
        {
            return (T)(object) Create(typeof(T), instance);
        }
        
        /// <summary>
        /// Create duck type proxy from an interface type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instance">Instance object</param>
        /// <returns>Duck Type proxy</returns>
        public static DuckType Create(Type interfaceType, object instance)
        {
            EnsureArguments(interfaceType, instance);

            // Create Type
            var type = GetOrCreateProxyType(interfaceType, instance.GetType()); 
            
            // Create instance
            var objInstance = (DuckType)FormatterServices.GetUninitializedObject(type);
            objInstance.Instance = instance;
            return objInstance;
        }
        
        /// <summary>
        /// Create a duck type proxy from an interface type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instanceType">Instance type</param>
        /// <returns>Duck Type proxy</returns>
        public static DuckType Create(Type interfaceType, Type instanceType)
        {
            var type = GetOrCreateProxyType(interfaceType, instanceType);
            return (DuckType) FormatterServices.GetUninitializedObject(type);
        }
    }
}