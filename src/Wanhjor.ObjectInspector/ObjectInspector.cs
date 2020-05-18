using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Object inspector
    /// </summary>
    public sealed class ObjectInspector
    {
        private readonly InspectName[] _names;
        private readonly Dictionary<Type, TypeStructure> _structures = new Dictionary<Type, TypeStructure>();
        private readonly bool _autoGrow;

        /// <summary>
        /// Creates a new object inspector
        /// </summary>
        public ObjectInspector()
        {
            _autoGrow = true;
            _names = Array.Empty<InspectName>();
        }

        /// <summary>
        /// Creates a new object inspector for a number of names
        /// </summary>
        /// <param name="names">Names to inspect inside an object</param>
        public ObjectInspector(params string[] names)
        {
            if (names is null)
                throw new ArgumentException("Names is null", nameof(names));
            _names = new InspectName[names.Length];
            for (var i = 0; i < names.Length; i++)
                _names[i] = new InspectName(names[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Creates a new object inspector for a number of names
        /// </summary>
        /// <param name="names">Names to inspect inside an object</param>
        public ObjectInspector(params InspectName[] names)
        {
            if (names is null)
                throw new ArgumentException("Names is null", nameof(names));
            _names = new InspectName[names.Length];
            for (var i = 0; i < names.Length; i++)
                _names[i] = names[i];
        }

        /// <summary>
        /// Gets the object data viewer for an object instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Object data viewer to inspect</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ObjectData With(object instance)
        {
            if (instance is null)
                return default;

            var iType = instance.GetType();
            if (_structures.TryGetValue(iType, out var structure))
                return structure.GetObjectData(instance);

            structure = new TypeStructure(_names, _autoGrow);
            _structures[iType] = structure;
            return structure.GetObjectData(instance);
        }

        /// <summary>
        /// Internal type structure
        /// </summary>
        internal sealed class TypeStructure
        {
            public readonly Dictionary<string, Fetcher?> Fetchers;
            public readonly bool AutoGrow;

            /// <summary>
            /// Creates an internal type structure based on the inspect names
            /// </summary>
            /// <param name="names">Names to inspect</param>
            /// <param name="autoGrow">Auto grow with new fetchers on demand</param>
            internal TypeStructure(InspectName[] names, bool autoGrow)
            {
                AutoGrow = autoGrow;
                Fetchers = new Dictionary<string, Fetcher?>();
                foreach (var name in names)
                    Fetchers[name.Name] = new DynamicFetcher(name.Name, name.BindingFlags);
            }

            /// <summary>
            /// Gets the object data viewer to inspect for an object instance
            /// </summary>
            /// <param name="instance">Object instance</param>
            /// <returns>Object data viewer to inspect</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ObjectData GetObjectData(object instance)
            {
                return new ObjectData(this, instance);
            }
        }

        /// <summary>
        /// Object data viewer struct
        /// </summary>
        public readonly struct ObjectData
        {
            private readonly TypeStructure _structure;
            private readonly object _instance;

            /// <summary>
            /// Creates a new object data viewer
            /// </summary>
            /// <param name="structure">Object type structure</param>
            /// <param name="instance">Object instance</param>
            internal ObjectData(TypeStructure structure, object instance)
            {
                _structure = structure;
                _instance = instance;
            }

            /// <summary>
            /// Try Get or Create Fetcher (if auto grow)
            /// </summary>
            /// <param name="name">Name</param>
            /// <param name="fetcher">Fetcher</param>
            /// <returns>True if the fetcher was found or created; otherwise, false</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetFetcher(string name, out Fetcher? fetcher)
            {
                if (_structure.Fetchers.TryGetValue(name, out fetcher))
                    return fetcher != null;

                if (!_structure.AutoGrow)
                    return false;

                var df = new DynamicFetcher(name);
                df.Load(_instance);
                fetcher = df.Type != FetcherType.None ? df : null;
                _structure.Fetchers[name] = fetcher;
                return fetcher != null;
            }

            /// <summary>
            /// Gets or sets values from the object instance
            /// </summary>
            /// <param name="name">Name to access in the object instance</param>
            /// <returns>Value of that name inside the object</returns>
            public object? this[string name]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (TryGetFetcher(name, out var fetcher))
                        return fetcher!.Fetch(_instance);
                    throw new KeyNotFoundException("Fetcher is null");
                }
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set
                {
                    if (TryGetFetcher(name, out var fetcher))
                        fetcher!.Shove(_instance, value);
                    else
                        throw new KeyNotFoundException("Fetcher is null");
                }
            }

            /// <summary>
            /// Tries to get a value from the object instance
            /// </summary>
            /// <param name="name">Name to access in the object instance</param>
            /// <param name="value">Value of that name inside the object</param>
            /// <returns>True if the name exist; otherwise, false</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetValue(string name, out object? value)
            {
                if (TryGetFetcher(name, out var fetcher) && fetcher != null)
                {
                    value = fetcher.Fetch(_instance);
                    return true;
                }
                value = null;
                return false;
            }

            /// <summary>
            /// Determines wether the object type structure contains the name
            /// </summary>
            /// <param name="name">Name to locate</param>
            /// <returns>True if the name exist in the type structure; otherwise, false.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsName(string name)
            {
                return TryGetFetcher(name, out _);
            }

            /// <summary>
            /// String representation of the object type structure with the values from the current object instance
            /// </summary>
            /// <returns>String value</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
            {
                var lst = new List<string>();
                foreach (var fetcher in _structure.Fetchers)
                    lst.Add(fetcher.Value is null ? $"{fetcher.Key}: (null)" : $"{fetcher.Key}: {fetcher.Value.Fetch(_instance)}");
                return "[" + string.Join(", ", lst) + "]";
            }
        }
    }
}
