using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Delegate Property Fetcher
    /// </summary>
    public sealed class DelegatePropertyFetcher<TDeclare, TValue>: Fetcher
    {
        private static readonly Func<TDeclare, TValue> EmptyGetter = (obj) => default!;
        private static readonly Action<TDeclare, TValue> EmptySetter = (obj, val) => { };
        private static readonly ConcurrentDictionary<PropertyInfo, Func<TDeclare, TValue>> Getters = new ConcurrentDictionary<PropertyInfo, Func<TDeclare, TValue>>();
        private static readonly ConcurrentDictionary<PropertyInfo, Action<TDeclare, TValue>> Setters = new ConcurrentDictionary<PropertyInfo, Action<TDeclare, TValue>>();
        private readonly Func<TDeclare, TValue> _getFunc;
        private readonly Action<TDeclare, TValue> _setFunc;
        
        /// <summary>
        /// Creates a new fetcher for a property
        /// </summary>
        /// <param name="property">Property info</param>
        public DelegatePropertyFetcher(PropertyInfo property) : base(property.Name)
        {
            Kind = FetcherKind.Property;
            _getFunc = Getters.GetOrAdd(property, prop =>
            {
                if (!prop.CanRead) return EmptyGetter;
                if (!prop.GetMethod.IsStatic) return (Func<TDeclare, TValue>) Delegate.CreateDelegate(typeof(Func<TDeclare, TValue>), prop.GetMethod);
                var stFunc = (Func<TValue>) Delegate.CreateDelegate(typeof(Func<TValue>), prop.GetMethod);
                return (obj) => stFunc();
            });
            _setFunc = Setters.GetOrAdd(property, prop =>
            {
                if (!prop.CanWrite) return EmptySetter;
                if (!prop.SetMethod.IsStatic) return (Action<TDeclare, TValue>) Delegate.CreateDelegate(typeof(Action<TDeclare, TValue>), prop.SetMethod);
                var stFunc = (Action<TValue>) Delegate.CreateDelegate(typeof(Action<TValue>), prop.SetMethod);
                return (obj, value) => stFunc(value);
            });
        }
        
        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object? Fetch(object? obj) => _getFunc((TDeclare)obj!);

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Shove(object? obj, object? value) => _setFunc((TDeclare)obj!, (TValue)value!);
        
        /// <summary>
        /// Invokes the method
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object? Invoke(object? obj, params object[] parameters) => null;
    }
}