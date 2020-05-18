using System;
using System.Diagnostics;
using System.Reflection;
// ReSharper disable HeapView.BoxingAllocation

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Fetcher for Properties
    /// </summary>
    /// <typeparam name="TObject">Object type</typeparam>
    /// <typeparam name="TProperty">Property type</typeparam>
    internal sealed class TypedFetchProperty<TObject, TProperty> : Fetcher
    {
        private readonly Func<TProperty>? _staticPropertyFetch;
        private readonly Func<TObject, TProperty>? _propertyFetch;
        private readonly Action<TObject, TProperty>? _propertyShove;
        private readonly Action<TProperty>? _staticPropertyShove;
        private readonly PropertyInfo _property;

        /// <summary>
        /// Creates a new fetcher for a property
        /// </summary>
        /// <param name="property">Property info</param>
        public TypedFetchProperty(PropertyInfo property) : base(property.Name)
        {
            Type = FetcherType.Property;
            _property = property;

            if (property.CanRead)
            {
                try
                {
                    var getMethod = property.GetMethod;
                    if (getMethod.IsStatic)
                    {
                        _staticPropertyFetch = (Func<TProperty>)getMethod.CreateDelegate(typeof(Func<TProperty>));
                    }
                    else
                    {
                        _propertyFetch = (Func<TObject, TProperty>)getMethod.CreateDelegate(typeof(Func<TObject, TProperty>));
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Log(1, "Warning", "Error creating property getter delegate: " + ex + "\n");
                }
            }

            if (property.CanWrite)
            {
                try
                {
                    var setMethod = property.SetMethod;
                    if (setMethod.IsStatic)
                    {
                        _staticPropertyShove = (Action<TProperty>)setMethod.CreateDelegate(typeof(Action<TProperty>));
                    }
                    else
                    {
                        _propertyShove = (Action<TObject, TProperty>)setMethod.CreateDelegate(typeof(Action<TObject, TProperty>));
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Log(1, "Warning", "Error creating property setter delegate: " + ex + "\n");
                }
            }
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        public override object? Fetch(object? obj)
        {
            if (_propertyFetch != null)
                return _propertyFetch((TObject)obj!);
            if (_staticPropertyFetch != null)
                return _staticPropertyFetch();
            return _property.CanRead ? _property.GetValue(obj) : null;
        }

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        public override void Shove(object? obj, object? value)
        {
            if (_propertyShove != null)
                _propertyShove((TObject)obj!, (TProperty)value!);
            else if (_staticPropertyShove != null)
                _staticPropertyShove((TProperty)value!);
            else if (_property.CanWrite)
                _property.SetValue(obj, value);
        }
    }
}
