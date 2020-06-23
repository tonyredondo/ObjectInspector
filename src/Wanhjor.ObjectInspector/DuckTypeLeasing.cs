using System.Collections.Concurrent;
using System.Threading;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck type leasing
    /// </summary>
    /// <typeparam name="TInterface">Interface type</typeparam>
    public ref struct DuckTypeLeasing<TInterface> where TInterface:class
    {
        private static readonly ConcurrentStack<TInterface> Proxies = new ConcurrentStack<TInterface>();
        private static TInterface? _firstItem;
        
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
            ((ISettableDuckType) inst).SetInstance(null!);
            if (_firstItem == null)
            {
                _firstItem = inst;
                return;
            }
            Proxies.Push(inst);
        }

        internal static DuckTypeLeasing<TInterface> Rent(IDuckTypeFactory<TInterface> factory, object instance)
        {
            var proxy = _firstItem;
            if (proxy != null && proxy == Interlocked.CompareExchange(ref _firstItem, null, proxy))
                ((ISettableDuckType)proxy).SetInstance(instance);
            else if (!Proxies.TryPop(out proxy))
                proxy = factory.Create(instance);
            else
                ((ISettableDuckType)proxy).SetInstance(instance);
            return new DuckTypeLeasing<TInterface>
            {
                Instance = proxy
            };
        }
        internal static DuckTypeLeasing<IDuckType> RentDuckType(IDuckTypeFactory factory, object instance)
        {
            var proxy = _firstItem;
            if (proxy != null && proxy == Interlocked.CompareExchange(ref _firstItem, null, proxy) && proxy is ISettableDuckType sProxy)
            {
                sProxy.SetInstance(instance);
                return new DuckTypeLeasing<IDuckType>
                {
                    Instance = sProxy
                };
            }

            if (!(Proxies.TryPop(out proxy) && proxy is ISettableDuckType dtProxy))
                dtProxy = (ISettableDuckType) factory.Create(instance);
            else
                dtProxy.SetInstance(instance);
            return new DuckTypeLeasing<IDuckType>
            {
                Instance = dtProxy
            };
        }
    }
}