using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Expression Tree Fetcher
    /// </summary>
    public sealed class EmitFetcher: Fetcher
    {
        private static readonly Func<object, object> EmptyGetter = (obj) => null!;
        private static readonly Action<object, object> EmptySetter = (obj, val) => { };
        private static readonly Func<object, object[], object> EmptyInvoker = (obj, args) => null!;
        private static readonly ConcurrentDictionary<MemberInfo, Func<object, object>> Getters = new ConcurrentDictionary<MemberInfo, Func<object, object>>();
        private static readonly ConcurrentDictionary<MemberInfo, Action<object, object>> Setters = new ConcurrentDictionary<MemberInfo, Action<object, object>>();
        private static readonly ConcurrentDictionary<MethodInfo, Func<object, object[], object>> Invokers = new ConcurrentDictionary<MethodInfo, Func<object, object[], object>>();
        private readonly Func<object, object> _getFunc;
        private readonly Action<object, object> _setFunc;
        private readonly Func<object, object[], object> _invoker;
        
        /// <summary>
        /// Creates a new fetcher for a property
        /// </summary>
        /// <param name="property">Property info</param>
        public EmitFetcher(PropertyInfo property) : base(property.Name)
        {
            Kind = FetcherKind.Property;
            _getFunc = Getters.GetOrAdd(property, prop => ((PropertyInfo)prop).CanRead ? EmitAccessors.BuildGetAccessor((PropertyInfo)prop) : EmptyGetter);
            _setFunc = Setters.GetOrAdd(property, prop => ((PropertyInfo)prop).CanWrite ? EmitAccessors.BuildSetAccessor((PropertyInfo)prop) : EmptySetter);
            _invoker = EmptyInvoker;
        }
        
        /// <summary>
        /// Creates a new fetcher for a field
        /// </summary>
        /// <param name="field">Field info</param>
        public EmitFetcher(FieldInfo field) : base(field.Name)
        {
            Kind = FetcherKind.Field;
            _getFunc = Getters.GetOrAdd(field, f => EmitAccessors.BuildGetAccessor((FieldInfo)f));
            _setFunc = Setters.GetOrAdd(field, f => (((FieldInfo)f).Attributes & FieldAttributes.InitOnly) == 0 ? EmitAccessors.BuildSetAccessor((FieldInfo)f) : EmptySetter);
            _invoker = EmptyInvoker;
        }
        
        /// <summary>
        /// Creates a new fetcher for a method
        /// </summary>
        /// <param name="method">Field info</param>
        public EmitFetcher(MethodInfo method) : base(method.Name)
        {
            Kind = FetcherKind.Method;
            _getFunc = EmptyGetter;
            _setFunc = EmptySetter;
            _invoker = Invokers.GetOrAdd(method, minfo => EmitAccessors.BuildMethodAccessor(minfo, false));
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
        
        /// <summary>
        /// Invokes the method
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object? Invoke(object? obj, params object[] parameters) => _invoker(obj!, parameters);
    }
}