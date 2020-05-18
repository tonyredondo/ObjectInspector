using System.Reflection;
// ReSharper disable UnusedMember.Global

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Inspector name
    /// </summary>
    public readonly struct InspectName
    {
        /// <summary>
        /// Name to inspect
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Binding flags
        /// </summary>
        public readonly BindingFlags? BindingFlags;

        /// <summary>
        /// Creates a new inspector name
        /// </summary>
        /// <param name="name">Name to inspect</param>
        public InspectName(string name)
        {
            Name = name;
            BindingFlags = null;
        }
        /// <summary>
        /// Creates a new inspector name
        /// </summary>
        /// <param name="name">Name to inspect</param>
        /// <param name="bindingFlags">Binding flags</param>
        public InspectName(string name, BindingFlags? bindingFlags)
        {
            Name = name;
            BindingFlags = bindingFlags;
        }
    }
}
