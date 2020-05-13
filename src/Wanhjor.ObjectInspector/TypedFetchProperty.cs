using System;
using System.Reflection;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Fetcher for Properties
    /// </summary>
    /// <typeparam name="TObject">Object type</typeparam>
    /// <typeparam name="TProperty">Property type</typeparam>
    class TypedFetchProperty<TObject, TProperty> : Fetcher
    {
        private readonly Func<TObject, TProperty> _propertyFetch;
        private readonly Action<TObject, TProperty> _propertyShove;
        private readonly PropertyInfo _property;

        /// <summary>
        /// Creates a new fetcher for a property
        /// </summary>
        /// <param name="property">Property info</param>
        public TypedFetchProperty(PropertyInfo property) : base(property.Name)
        {
            _property = property;
            try
            {
                _propertyFetch = (Func<TObject, TProperty>)property.GetMethod.CreateDelegate(typeof(Func<TObject, TProperty>));
            }
            catch
            {
                // Can't create the delegate
            }

            try
            {
                _propertyShove = (Action<TObject, TProperty>)property.SetMethod.CreateDelegate(typeof(Action<TObject, TProperty>));
            }
            catch
            {
                // Can't create the delegate
            }
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        public override object Fetch(object obj)
        {
            if (_propertyFetch != null)
                return _propertyFetch((TObject)obj);
            else if (_property.CanRead)
                return _property.GetValue(obj);
            return null;
        }

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        public override void Shove(object obj, object value)
        {
            if (_propertyShove != null)
                _propertyShove((TObject)obj, (TProperty)value);
            else if (_property.CanWrite)
                _property.SetValue(obj, value);
        }
    }
}
