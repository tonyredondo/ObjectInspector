using System;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck type interface
    /// </summary>
    public interface IDuckType
    {
        /// <summary>
        /// Instance
        /// </summary>
        object? Instance { get; }
        /// <summary>
        /// Instance Type
        /// </summary>
        Type? Type { get; }
        /// <summary>
        /// Assembly version
        /// </summary>
        Version? AssemblyVersion { get; }
    }

    /// <summary>
    /// Settable duck type interface
    /// </summary>
    public interface ISettableDuckType : IDuckType
    {
        /// <summary>
        /// Sets the instance object
        /// </summary>
        /// <param name="instance">Object instance value</param>
        void SetInstance(object instance);
    }
}