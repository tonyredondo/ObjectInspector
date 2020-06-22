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
        /// <typeparam name="T">Type of Duck</typeparam>
        /// <returns>Duck Type factory</returns>
        public static IDuckTypeFactory<T> GetFactory<T>(object instance) where T:class
        {
            var duckType = typeof(T);
            EnsureArguments(duckType, instance);

            // Create Type
            var type = GetOrCreateProxyType(duckType, instance.GetType());
            return new DuckTypeFactory<T>(type);
        }
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="duckType">Duck type</param>
        /// <param name="instanceType">Object type</param>
        /// <returns>Duck type factory</returns>
        public static IDuckTypeFactory<object> GetFactoryByTypes(Type duckType, Type instanceType)
        {
            var type = GetOrCreateProxyType(duckType, instanceType);
            return new DuckTypeFactory<object>(type);
        }
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="instanceType">Type of instance</param>
        /// <typeparam name="T">Type of Duck</typeparam>
        /// <returns>Duck Type factory</returns>
        public static IDuckTypeFactory<T> GetFactoryByTypes<T>(Type instanceType) where T:class
        {
            var duckType = typeof(T);
            var type = GetOrCreateProxyType(duckType, instanceType);
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
                var inst = (ISettableDuckType) FormatterServices.GetUninitializedObject(_proxyType);
                inst.SetInstance(instance);
                return (inst as T)!;
            }
            public DuckTypeLeasing<T> Rent(object instance)
                => DuckTypeLeasing<T>.Rent(this, instance);
            
            IDuckType IDuckTypeFactory.Create(object instance)
            {
                var inst = (ISettableDuckType) FormatterServices.GetUninitializedObject(_proxyType);
                inst.SetInstance(instance);
                return inst;
            }
            DuckTypeLeasing<IDuckType> IDuckTypeFactory.Rent(object instance)
                => DuckTypeLeasing<IDuckType>.RentDuckType(this, instance);
        }
    }
}