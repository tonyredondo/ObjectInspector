using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Dynamic fetcher
    /// </summary>
    public class DynamicFetcher : Fetcher
    {
        private readonly BindingFlags? _bindingFlags;
        private Fetcher _fetcher;

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
        /// Create a fetcher for the name in the object
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Fetcher</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Fetcher CreateFetcher(object obj)
        {
            var typeInfo = obj.GetType().GetTypeInfo();
            var pInfo = _bindingFlags.HasValue ? typeInfo.GetProperty(Name, _bindingFlags.Value) : typeInfo.GetDeclaredProperty(Name) ?? typeInfo.GetRuntimeProperty(Name);
            if (!(pInfo is null))
                return FetcherForProperty(pInfo);

            var fInfo = _bindingFlags.HasValue ? typeInfo.GetField(Name, _bindingFlags.Value) : typeInfo.GetDeclaredField(Name) ?? typeInfo.GetRuntimeField(Name);
            if (!(fInfo is null))
                return FetcherForField(fInfo);

            return new Fetcher(Name);
        }

        /// <summary>
        /// Load fetcher
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Load(object obj)
        {
            if (_fetcher is null)
            {
                _fetcher = CreateFetcher(obj);
                Type = _fetcher.Type;
            }
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object Fetch(object obj)
        {
            Load(obj);
            return _fetcher.Fetch(obj);
        }

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Shove(object obj, object value)
        {
            Load(obj);
            _fetcher.Shove(obj, value);
        }
    }
}
