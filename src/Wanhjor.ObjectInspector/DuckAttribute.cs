using System;
using System.Reflection;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
    public class DuckAttribute : Attribute
    {
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