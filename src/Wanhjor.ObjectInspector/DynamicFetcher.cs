using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Dynamic fetcher
    /// </summary>
    public sealed class DynamicFetcher : Fetcher
    {
        private static readonly ConcurrentDictionary<(string Name, Type Type), Fetcher> Fetchers = new ConcurrentDictionary<(string, Type), Fetcher>();
        private readonly BindingFlags? _bindingFlags;
        private readonly Func<MethodInfo, bool>? _methodSelector = null;
        private Fetcher _fetcher = null!;
        private FetcherType _fetcherType = FetcherType.ExpressionTree;
        
        /// <summary>
        /// Gets or sets the fetcher Type
        /// </summary>
        public FetcherType FetcherType
        {
            get => _fetcherType;
            set
            {
                _fetcherType = value;
                _fetcher = null!;
            }
        }

        #region .ctor
        /// <summary>
        /// Efficient implementation of fetching properties and fields of anonymous types with reflection.
        /// </summary>
        /// <param name="name">Name</param>
        public DynamicFetcher(string name) : base(name)
        {
        }

        /// <summary>
        /// Efficient implementation of fetching properties and fields of anonymous types with reflection.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        public DynamicFetcher(string name, BindingFlags? bindingFlags) : base(name)
        {
            _bindingFlags = bindingFlags;
        }

        /// <summary>
        /// Efficient implementation of fetching properties and fields of anonymous types with reflection.
        /// </summary>
        /// <param name="inspectName"> Inspect Name</param>
        public DynamicFetcher(InspectName inspectName) : base(inspectName.Name)
        {
            _bindingFlags = inspectName.BindingFlags;
        }
        
        /// <summary>
        /// Efficient implementation of fetching properties and fields of anonymous types with reflection.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="methodSelector">Method selector predicate</param>
        public DynamicFetcher(string name, Func<MethodInfo, bool> methodSelector) : base(name)
        {
            _methodSelector = methodSelector;
        }

        /// <summary>
        /// Efficient implementation of fetching properties and fields of anonymous types with reflection.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="bindingFlags">Binding flags</param>
        /// <param name="methodSelector">Method selector predicate</param>
        public DynamicFetcher(string name, BindingFlags? bindingFlags, Func<MethodInfo, bool> methodSelector) : base(name)
        {
            _bindingFlags = bindingFlags;
            _methodSelector = methodSelector;
        }

        /// <summary>
        /// Efficient implementation of fetching properties and fields of anonymous types with reflection.
        /// </summary>
        /// <param name="inspectName"> Inspect Name</param>
        /// <param name="methodSelector">Method selector predicate</param>
        public DynamicFetcher(InspectName inspectName, Func<MethodInfo, bool> methodSelector) : base(inspectName.Name)
        {
            _bindingFlags = inspectName.BindingFlags;
            _methodSelector = methodSelector;
        }
        #endregion 
        
        /// <summary>
        /// Create a fetcher for the name in the object
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Fetcher</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Fetcher CreateFetcher(object? obj)
        {
            if (obj is null)
                return new Fetcher(string.Empty);

            return Fetchers.GetOrAdd((Name, obj.GetType()), t =>
            {
                var name = t.Name;
                var typeInfo = t.Type.GetTypeInfo();

                var pInfo = _bindingFlags.HasValue ? typeInfo.GetProperty(name, _bindingFlags.Value) : typeInfo.GetDeclaredProperty(name) ?? typeInfo.GetRuntimeProperty(name);
                if (!(pInfo is null))
                    return _fetcherType switch
                    {
                        FetcherType.ExpressionTree => new ExpressionTreeFetcher(pInfo),
                        FetcherType.Emit => new EmitFetcher(pInfo),
                        _ => new Fetcher(Name)
                    };

                var fInfo = _bindingFlags.HasValue ? typeInfo.GetField(name, _bindingFlags.Value) : typeInfo.GetDeclaredField(name) ?? typeInfo.GetRuntimeField(name);
                if (!(fInfo is null))
                    return _fetcherType switch
                    {
                        FetcherType.ExpressionTree => new ExpressionTreeFetcher(fInfo),
                        FetcherType.Emit => new EmitFetcher(fInfo),
                        _ => new Fetcher(Name)
                    };


                MethodInfo[] methods;
                if (_bindingFlags.HasValue)
                {
                    methods = typeInfo.GetMethods(_bindingFlags.Value);
                }
                else
                {
                    methods = typeInfo.GetDeclaredMethods(name).ToArray();
                    if (methods.Length == 0)
                        methods = typeInfo.GetRuntimeMethods().ToArray();
                }

                MethodInfo? mInfo;
                if (_methodSelector != null && methods.Length > 0)
                    mInfo = methods.FirstOrDefault(_methodSelector);
                else
                    mInfo = methods.FirstOrDefault(m => m.Name == name);
                
                if (!(mInfo is null))
                    return _fetcherType switch
                    {
                        FetcherType.ExpressionTree => new ExpressionTreeFetcher(mInfo),
                        FetcherType.Emit => new EmitFetcher(mInfo),
                        _ => new Fetcher(Name)
                    };
            
                return new Fetcher(t.Name);
                
            });
            
        }

        /// <summary>
        /// Load fetcher
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Load(object? obj)
        {
            _fetcher = CreateFetcher(obj);
            Kind = _fetcher.Kind;
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object? Fetch(object? obj)
        {
            if (_fetcher is null)
                Load(obj);
            return _fetcher!.Fetch(obj);
        }

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Shove(object? obj, object? value)
        {
            if (_fetcher is null)
                Load(obj);
            _fetcher!.Shove(obj, value);
        }

        /// <summary>
        /// Invokes the method
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object? Invoke(object? obj, params object[] parameters)
        {
            if (_fetcher is null)
                Load(obj);
            return _fetcher!.Invoke(obj, parameters);
        }
    }
}
