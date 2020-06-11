using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Dynamic Fetcher Extensions
    /// </summary>
    public static class DynamicFetcherExtensions
    {
        private static readonly ConcurrentDictionary<VTuple<Type, string>, DynamicFetcher> Fetchers = new ConcurrentDictionary<VTuple<Type, string>, DynamicFetcher>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetDynamicFetcher(object obj, string propertyOrFieldName, out DynamicFetcher? fetcher)
        {
            fetcher = null;
            if (obj is null) return false;
            if (string.IsNullOrWhiteSpace(propertyOrFieldName)) return false;
            var objType = obj.GetType();
            fetcher = Fetchers.GetOrAdd(new VTuple<Type, string>(objType, propertyOrFieldName),
                tuple => new DynamicFetcher(tuple.Item2, DuckAttribute.AllFlags));
            fetcher.Load(obj);
            return fetcher.Kind != FetcherKind.None;
        }

        
        /// <summary>
        /// Tries to get a property or field value using a cached dynamic fetcher for the type
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="propertyOrFieldName">Property or field name</param>
        /// <param name="value">Property or field value</param>
        /// <returns>True if the value could be retrieved; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetMemberValue(this object obj, string propertyOrFieldName, out object? value)
        {
            if (!TryGetDynamicFetcher(obj, propertyOrFieldName, out var fetcher))
            {
                value = null;
                return false;
            }
            value = fetcher!.Fetch(obj);
            return true;
        }
        
        /// <summary>
        /// Tries to set a property or field value using a cached dynamic fetcher for the type
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="propertyOrFieldName">Property or field name</param>
        /// <param name="value">Property or field value</param>
        /// <returns>True if the value could be set; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetMemberValue(this object obj, string propertyOrFieldName, object value)
        {
            if (!TryGetDynamicFetcher(obj, propertyOrFieldName, out var fetcher)) return false;
            fetcher!.Shove(obj, value);
            return true;
        }

        /// <summary>
        /// Tries to set a property or field value using a cached dynamic fetcher for the type
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="methodName">Method name</param>
        /// <param name="arguments">Method arguments value</param>
        /// <param name="returnValue">Return value</param>
        /// <returns>True if the method could be executed; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryInvokeMethod(this object obj, string methodName, object[] arguments, out object? returnValue)
        {
            if (!TryGetDynamicFetcher(obj, methodName, out var fetcher))
            {
                returnValue = null;
                return false;
            }
            returnValue = fetcher!.Invoke(obj, arguments);
            return true;
        }
        
        /// <summary>
        /// Tries to set a property or field value using a cached dynamic fetcher for the type
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="methodName">Method name</param>
        /// <param name="arguments">Method arguments value</param>
        /// <returns>True if the method could be executed; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryInvokeMethod(this object obj, string methodName, params object[] arguments)
        {
            if (!TryGetDynamicFetcher(obj, methodName, out var fetcher))
                return false;
            fetcher!.Invoke(obj, arguments);
            return true;
        }
    }
}
