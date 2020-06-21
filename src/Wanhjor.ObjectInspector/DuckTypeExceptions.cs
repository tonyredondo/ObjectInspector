using System;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// DuckType property can't be read
    /// </summary>
    public class DuckTypePropertyCantBeReadException : Exception
    {
        /// <summary>
        /// DuckType property can't be read
        /// </summary>
        public DuckTypePropertyCantBeReadException() : base("The property can't be read, you should remove the getter from the interface."){}
    }
    /// <summary>
    /// DuckType property can't be written
    /// </summary>
    public class DuckTypePropertyCantBeWrittenException : Exception
    {
        /// <summary>
        /// DuckType property can't be written
        /// </summary>
        public DuckTypePropertyCantBeWrittenException() : base("The property can't be written, you should remove the setter from the interface.") {}
    }
    /// <summary>
    /// DuckType field is readonly
    /// </summary>
    public class DuckTypeFieldIsReadonlyException : Exception
    {
        /// <summary>
        /// DuckType field is readonly
        /// </summary>
        public DuckTypeFieldIsReadonlyException() : base("The field is marked as readonly, you should remove the setter from the interface.") {}
    }
    /// <summary>
    /// DuckType property or field not found
    /// </summary>
    public class DuckTypePropertyOrFieldNotFoundException : Exception
    {
        /// <summary>
        /// DuckType property or field not found
        /// </summary>
        public DuckTypePropertyOrFieldNotFoundException() : base("The property or field was not found in the instance.") {}
    }
    /// <summary>
    /// DuckType type is not an interface exception
    /// </summary>
    public class DuckTypeTypeIsNotValidException : Exception
    {
        /// <summary>
        /// DuckType type is not valid exception
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="argumentName">Name of the argument</param>
        public DuckTypeTypeIsNotValidException(Type type, string argumentName) : base($"The type '{type.FullName}' is not a valid type, argument: '{argumentName}'") {}
    }
    /// <summary>
    /// DuckType type is not public exception
    /// </summary>
    public class DuckTypeTypeIsNotPublicException : Exception
    {
        /// <summary>
        /// DuckType type is not public exception
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="argumentName">Name of the argument</param>
        public DuckTypeTypeIsNotPublicException(Type type, string argumentName) : base($"The type '{type.FullName}' must be public, argument: '{argumentName}'") {}
    }
}