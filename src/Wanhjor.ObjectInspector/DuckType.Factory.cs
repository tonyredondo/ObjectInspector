using System;
using System.Runtime.Serialization;

namespace Wanhjor.ObjectInspector
{
    public partial class DuckType
    {
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <typeparam name="T">Type of interface</typeparam>
        /// <returns>Duck Type factory</returns>
        public static IDuckTypeFactory<T> GetFactory<T>(object instance) where T:class
        {
            var interfaceType = typeof(T);
            EnsureArguments(interfaceType, instance);

            // Create Type
            var type = GetOrCreateProxyType(interfaceType, instance.GetType());
            return new DuckTypeFactory<T>(type);
        }
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instanceType">Object type</param>
        /// <returns>Duck type factory</returns>
        public static IDuckTypeFactory<object> GetFactoryByTypes(Type interfaceType, Type instanceType)
        {
            var type = GetOrCreateProxyType(interfaceType, instanceType);
            return new DuckTypeFactory<object>(type);
        }
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="instanceType">Type of instance</param>
        /// <typeparam name="T">Type of interface</typeparam>
        /// <returns>Duck Type factory</returns>
        public static IDuckTypeFactory<T> GetFactoryByTypes<T>(Type instanceType) where T:class
        {
            var interfaceType = typeof(T);
            var type = GetOrCreateProxyType(interfaceType, instanceType);
            return new DuckTypeFactory<T>(type);
        }
        

        class DuckTypeFactory<T> : IDuckTypeFactory<T>, IDuckTypeFactory where T:class
        {
            private readonly Type _proxyType;
            internal DuckTypeFactory(Type proxyType)
            {
                _proxyType = proxyType;
            }
            
            public T Create(object instance)
            {
                var inst = (DuckType) FormatterServices.GetUninitializedObject(_proxyType);
                inst.Instance = instance;
                return (inst as T)!;
            }
            public DuckTypeLeasing<T> Rent(object instance)
                => DuckTypeLeasing<T>.Rent(this, instance);
            
            DuckType IDuckTypeFactory.Create(object instance)
            {
                var inst = (DuckType) FormatterServices.GetUninitializedObject(_proxyType);
                inst.Instance = instance;
                return inst;
            }
            DuckTypeLeasing<DuckType> IDuckTypeFactory.Rent(object instance)
                => DuckTypeLeasing<DuckType>.RentDuckType(this, instance);
        }
    }
}