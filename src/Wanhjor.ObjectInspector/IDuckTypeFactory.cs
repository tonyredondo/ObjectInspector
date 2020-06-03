namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck Type factory
    /// </summary>
    public interface IDuckTypeFactory<TInterface> where TInterface:class
    {
        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        TInterface Create(object instance);
        /// <summary>
        /// Rent a duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>DuckType leasing instance</returns>
        DuckTypeLeasing<TInterface> Rent(object instance);
    }

    /// <summary>
    /// Duck Type factory
    /// </summary>
    public interface IDuckTypeFactory
    {
        /// <summary>
        /// Create duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Duck type proxy instance</returns>
        DuckType Create(object instance);
        /// <summary>
        /// Rent a duck type proxy instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <returns>DuckType leasing instance</returns>
        DuckTypeLeasing<DuckType> Rent(object instance);
    }
}