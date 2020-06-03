using System.Collections.Concurrent;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck type leasing
    /// </summary>
    /// <typeparam name="TInterface">Interface type</typeparam>
    public ref struct DuckTypeLeasing<TInterface> where TInterface:class
    {
        private static readonly ConcurrentStack<TInterface> Proxies = new ConcurrentStack<TInterface>();
        
        /// <summary>
        /// Current duck type instance
        /// </summary>
        public TInterface Instance { get; private set; }
        
        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            if (Instance is null) return;
            var inst = Instance;
            Instance = default!;
            ((DuckType) (object) inst).Instance = null;
            Proxies.Push(inst);
        }

        internal static DuckTypeLeasing<TInterface> Rent(IDuckTypeFactory<TInterface> factory, object instance)
        {
            if (!Proxies.TryPop(out var proxy))
                proxy = factory.Create(instance);
            else
                ((DuckType) (object) proxy).Instance = instance;
            return new DuckTypeLeasing<TInterface>
            {
                Instance = proxy
            };
        }
        internal static DuckTypeLeasing<DuckType> RentDuckType(IDuckTypeFactory factory, object instance)
        {
            if (!(Proxies.TryPop(out var proxy) && proxy is DuckType dtProxy))
                dtProxy = factory.Create(instance);
            else
                dtProxy.Instance = instance;
            return new DuckTypeLeasing<DuckType>
            {
                Instance = dtProxy
            };
        }
    }
}