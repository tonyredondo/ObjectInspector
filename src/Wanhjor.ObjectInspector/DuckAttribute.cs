using System;
using System.Reflection;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class DuckAttribute : Attribute
    {
        private string? _upToVersion;
        
        /// <summary>
        /// Property Name
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Binding flags
        /// </summary>
        public BindingFlags Flags { get; set; } = BindingFlags.Instance | BindingFlags.Public;
        /// <summary>
        /// Duck kind
        /// </summary>
        public DuckKind Kind { get; set; } = DuckKind.Property;

        /// <summary>
        /// Up to assembly version
        /// </summary>
        public string? UpToVersion
        {
            get => _upToVersion;
            set
            {
                Version = string.IsNullOrWhiteSpace(value) ? null : new Version(value);
                _upToVersion = value;
            }
        }
        /// <summary>
        /// Internal up to assembly version
        /// </summary>
        internal Version? Version { get; private set; }
    }

    /// <summary>
    /// Duck kind
    /// </summary>
    public enum DuckKind
    {
        /// <summary>
        /// Property
        /// </summary>
        Property,
        /// <summary>
        /// Field
        /// </summary>
        Field
    }
}