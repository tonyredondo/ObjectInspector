using System;
using System.Reflection;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Fetcher base class
    /// </summary>
    public class Fetcher
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// .ctor
        /// </summary>
        internal Fetcher(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        public virtual object Fetch(object obj) => null;

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        public virtual void Shove(object obj, object value) { }

        /// <summary>
        /// Create a property fetcher from a .NET Reflection PropertyInfo class that
        /// represents a property of a particular type.  
        /// </summary>
        internal static Fetcher FetcherForProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                return new Fetcher(null);

            var typedPropertyFetcher = typeof(TypedFetchProperty<,>);
            var instantiatedTypedPropertyFetcher = typedPropertyFetcher.GetTypeInfo().MakeGenericType(
                propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (Fetcher)Activator.CreateInstance(instantiatedTypedPropertyFetcher, propertyInfo);
        }

        /// <summary>
        /// Create a property fetcher from a .NET Reflection PropertyInfo class that
        /// represents a property of a particular type.  
        /// </summary>
        internal static Fetcher FetcherForField(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
                return new Fetcher(null);

            var typedFieldFetcher = typeof(TypedFetchField<,>);
            var instantiatedTypedFieldFetcher = typedFieldFetcher.GetTypeInfo().MakeGenericType(
                fieldInfo.DeclaringType, fieldInfo.FieldType);
            return (Fetcher)Activator.CreateInstance(instantiatedTypedFieldFetcher, fieldInfo);
        }
    }
}
