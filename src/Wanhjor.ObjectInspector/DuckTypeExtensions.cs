using System;
using System.Runtime.CompilerServices;
using Wanhjor.ObjectInspector;
// ReSharper disable CheckNamespace
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

/// <summary>
/// Duck type extensions
/// </summary>
public static class DuckTypeExtensions
{
    /// <summary>
    /// Gets the duck type factory for the type implementing an interface T
    /// </summary>
    /// <param name="source">Source type</param>
    /// <typeparam name="T">Interface type</typeparam>
    /// <returns>IDuckTypeFactory instance</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDuckTypeFactory<T> DuckFactoryAs<T>(this Type source) where T: class
        => DuckType.GetFactoryByTypes<T>(source);
    /// <summary>
    /// Gets the duck type factory for the type implementing an interface T
    /// </summary>
    /// <param name="source">Source type</param>
    /// <param name="interfaceType">Interface type</param>
    /// <returns>IDuckTypeFactory instance</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDuckTypeFactory<object> DuckFactoryAs(this Type source, Type interfaceType)
        => DuckType.GetFactoryByTypes(interfaceType, source);

    /// <summary>
    /// Gets the duck type instance for the object implementing an interface T
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <typeparam name="T">Interface type</typeparam>
    /// <returns>DuckType instance</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T DuckAs<T>(this object instance) where T : class 
        => DuckType.Create<T>(instance);

    /// <summary>
    /// Gets the duck type instance for the object implementing an interface T
    /// </summary>
    /// <param name="instance">Object instance</param>
    /// <param name="interfaceType">Interface type</param>
    /// <returns>DuckType instance</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object DuckAs(this object instance, Type interfaceType)
        => DuckType.Create(interfaceType, instance);
}