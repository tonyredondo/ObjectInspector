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
        public static bool TryGetMemberValue<TValue>(this object obj, string propertyOrFieldName, out TValue value)
        {
            return TryGetMemberValue(obj, propertyOrFieldName, out value, out var ex);
        }
        /// <summary>
        /// Tries to get a property or field value using a cached dynamic fetcher for the type
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="propertyOrFieldName">Property or field name</param>
        /// <param name="value">Property or field value</param>
        /// <param name="exception">Exception value</param>
        /// <returns>True if the value could be retrieved; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetMemberValue<TValue>(this object obj, string propertyOrFieldName, out TValue value, out Exception? exception)
        {
            exception = null;
            if (!TryGetDynamicFetcher(obj, propertyOrFieldName, out var fetcher))
            {
                value = default!;
                return false;
            }
            try
            {
                value = (TValue) fetcher!.Fetch(obj)!;
            }
            catch (Exception ex)
            {
                value = default!;
                exception = ex;
                return false;
            }
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
            return TrySetMemberValue(obj, propertyOrFieldName, value, out var exception);
        }
        /// <summary>
        /// Tries to set a property or field value using a cached dynamic fetcher for the type
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="propertyOrFieldName">Property or field name</param>
        /// <param name="value">Property or field value</param>
        /// <param name="exception">Exception value</param>
        /// <returns>True if the value could be set; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TrySetMemberValue(this object obj, string propertyOrFieldName, object value, out Exception? exception)
        {
            exception = null;
            if (!TryGetDynamicFetcher(obj, propertyOrFieldName, out var fetcher)) return false;
            try
            {
                fetcher!.Shove(obj, value);
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
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
        public static bool TryInvokeMethod<TReturn>(this object obj, string methodName, object[] arguments, out TReturn returnValue)
        {
            return TryInvokeMethod(obj, methodName, arguments, out returnValue, out var exception);
        }
        
        /// <summary>
        /// Tries to set a property or field value using a cached dynamic fetcher for the type
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="methodName">Method name</param>
        /// <param name="arguments">Method arguments value</param>
        /// <param name="returnValue">Return value</param>
        /// <param name="exception">Exception value</param>
        /// <returns>True if the method could be executed; otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryInvokeMethod<TReturn>(this object obj, string methodName, object[] arguments, out TReturn returnValue, out Exception? exception)
        {
            exception = null;
            if (!TryGetDynamicFetcher(obj, methodName, out var fetcher))
            {
                returnValue = default!;
                return false;
            }
            try
            {
                returnValue = (TReturn)fetcher!.Invoke(obj, arguments)!;
            }
            catch (Exception ex)
            {
                returnValue = default!;
                exception = ex;
                return false;
            }
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
