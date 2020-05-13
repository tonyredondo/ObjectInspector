using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Object inspector
    /// </summary>
    public class ObjectInspector
    {
        private readonly InspectName[] _names;
        private readonly Dictionary<Type, TypeStructure> _structures = new Dictionary<Type, TypeStructure>();

        /// <summary>
        /// Creates a new object inspector for a number of names
        /// </summary>
        /// <param name="names">Names to inspect inside an object</param>
        public ObjectInspector(params string[] names)
        {
            Contract.Requires(!(names is null));
            _names = new InspectName[names.Length];
            for (var i=0; i<names.Length;i++)
                _names[i] = new InspectName(names[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Creates a new object inspector for a number of names
        /// </summary>
        /// <param name="names">Names to inspect inside an object</param>
        public ObjectInspector(params InspectName[] names)
        {
            Contract.Requires(!(names is null));
            _names = new InspectName[names.Length];
            for (var i = 0; i < names.Length; i++)
                _names[i] = names[i];
        }

        /// <summary>
        /// Gets the object data viewer for an object instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Object data viewer to inspect</returns>
        public ObjectData With(object instance)
        {
            if (instance is null)
                return default;

            var iType = instance.GetType();
            if (!_structures.TryGetValue(iType, out var structure))
            {
                structure = new TypeStructure(_names);
                _structures[iType] = structure;
            }

            return structure.GetObjectData(instance);
        }

        /// <summary>
        /// Internal type structure
        /// </summary>
        internal class TypeStructure
        {
            public Dictionary<string, Fetcher> Fetchers = new Dictionary<string, Fetcher>();

            /// <summary>
            /// Creates an internal type structure based on the inspect names
            /// </summary>
            /// <param name="names">Names to inspect</param>
            internal TypeStructure(InspectName[] names)
            {
                foreach (var name in names)
                    Fetchers[name.Name] = new DynamicFetcher(name.Name, name.BindingFlags);
            }

            /// <summary>
            /// Gets the object data viewer to inspect for an object instance
            /// </summary>
            /// <param name="instance">Object instance</param>
            /// <returns>Object data viewer to inspect</returns>
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
            /// Gets or sets values from the object instance
            /// </summary>
            /// <param name="name">Name to access in the object instance</param>
            /// <returns>Value of that name inside the object</returns>
            public object this[string name]
            {
                get => _structure.Fetchers[name].Fetch(_instance);
                set => _structure.Fetchers[name].Shove(_instance, value);
            }

            /// <summary>
            /// Tries to get a value from the object instance
            /// </summary>
            /// <param name="name">Name to access in the object instance</param>
            /// <param name="value">Value of that name inside the object</param>
            /// <returns>True if the name exist; otherwise, false</returns>
            public bool TryGetValue(string name, out object value)
            {
                if (_structure.Fetchers.TryGetValue(name, out var fetcher))
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
            public bool ContainsName(string name)
            {
                return _structure.Fetchers.ContainsKey(name);
            }

            /// <summary>
            /// String representation of the object type structure with the values from the current object instance
            /// </summary>
            /// <returns>String value</returns>
            public override string ToString()
            {
                var lst = new List<string>();
                foreach (var fetcher in _structure.Fetchers)
                    lst.Add($"{fetcher.Key}: {fetcher.Value.Fetch(_instance)}");
                return "[" + string.Join(", ", lst) + "]";
            }
        }
    }
}
