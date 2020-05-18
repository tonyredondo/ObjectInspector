using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Expression Tree Fetcher
    /// </summary>
    public sealed class ExpressionTreeFetcher: Fetcher
    {
        private static readonly ConcurrentDictionary<MemberInfo, Func<object, object>> Getters = new ConcurrentDictionary<MemberInfo, Func<object, object>>();
        private static readonly ConcurrentDictionary<MemberInfo, Action<object, object>> Setters = new ConcurrentDictionary<MemberInfo, Action<object, object>>();
        private readonly Func<object, object> _getFunc;
        private readonly Action<object, object> _setFunc;
        
        /// <summary>
        /// Creates a new fetcher for a property
        /// </summary>
        /// <param name="property">Property info</param>
        public ExpressionTreeFetcher(PropertyInfo property) : base(property.Name)
        {
            Type = FetcherType.Property;
            _getFunc = Getters.GetOrAdd(property, prop => ((PropertyInfo)prop).CanRead ? ExpressionAccessors.BuildGetAccessor((PropertyInfo)prop) : (obj) => null!);
            _setFunc = Setters.GetOrAdd(property, prop => ((PropertyInfo)prop).CanWrite ? ExpressionAccessors.BuildSetAccessor((PropertyInfo)prop) : (obj, val) => { });
        }
        
        /// <summary>
        /// Creates a new fetcher for a field
        /// </summary>
        /// <param name="field">Field info</param>
        public ExpressionTreeFetcher(FieldInfo field) : base(field.Name)
        {
            Type = FetcherType.Field;
            _getFunc = Getters.GetOrAdd(field, f => ExpressionAccessors.BuildGetAccessor((FieldInfo)f));
            _setFunc = Setters.GetOrAdd(field, f => (((FieldInfo)f).Attributes & FieldAttributes.InitOnly) == 0 ? ExpressionAccessors.BuildSetAccessor((FieldInfo)f) : (obj, val) => {});
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object? Fetch(object? obj) => _getFunc(obj!);

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Shove(object? obj, object? value) => _setFunc(obj!, value!);
        
    }
}