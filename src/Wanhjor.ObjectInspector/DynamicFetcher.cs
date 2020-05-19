using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Dynamic fetcher
    /// </summary>
    public sealed class DynamicFetcher : Fetcher
    {
        private readonly BindingFlags? _bindingFlags;
        private Fetcher _fetcher = null!;

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
        /// Create a fetcher for the name in the object
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Fetcher</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Fetcher CreateFetcher(object? obj)
        {
            if (obj is null)
                return new Fetcher(string.Empty);
            
            var typeInfo = obj.GetType().GetTypeInfo();
            var pInfo = _bindingFlags.HasValue ? typeInfo.GetProperty(Name, _bindingFlags.Value) : typeInfo.GetDeclaredProperty(Name) ?? typeInfo.GetRuntimeProperty(Name);
            if (!(pInfo is null))
                return new ExpressionTreeFetcher(pInfo);

            var fInfo = _bindingFlags.HasValue ? typeInfo.GetField(Name, _bindingFlags.Value) : typeInfo.GetDeclaredField(Name) ?? typeInfo.GetRuntimeField(Name);
            return !(fInfo is null) ? new ExpressionTreeFetcher(fInfo) : new Fetcher(Name);
        }

        /// <summary>
        /// Load fetcher
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Load(object? obj)
        {
            _fetcher = CreateFetcher(obj);
            Type = _fetcher.Type;
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
