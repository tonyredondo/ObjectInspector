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
        /// <typeparam name="T">Duck type</typeparam>
        /// <returns>Duck type proxy</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Create<T>(object instance)
        {
            return (T)(object) Create(typeof(T), instance);
        }
        
        /// <summary>
        /// Create duck type proxy from an interface type
        /// </summary>
        /// <param name="duckType">Duck type</param>
        /// <param name="instance">Instance object</param>
        /// <returns>Duck Type proxy</returns>
        public static DuckType Create(Type duckType, object instance)
        {
            EnsureArguments(duckType, instance);

            // Create Type
            var type = GetOrCreateProxyType(duckType, instance.GetType()); 
            
            // Create instance
            var objInstance = (DuckType)FormatterServices.GetUninitializedObject(type);
            objInstance.Instance = instance;
            return objInstance;
        }
        
        /// <summary>
        /// Create a duck type proxy from an interface type
        /// </summary>
        /// <param name="duckType">Duck type</param>
        /// <param name="instanceType">Instance type</param>
        /// <returns>Duck Type proxy</returns>
        public static DuckType Create(Type duckType, Type instanceType)
        {
            var type = GetOrCreateProxyType(duckType, instanceType);
            return (DuckType) FormatterServices.GetUninitializedObject(type);
        }
    }
}