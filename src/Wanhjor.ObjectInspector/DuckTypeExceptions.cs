using System;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// DuckType property can't be read
    /// </summary>
    public class DuckTypePropertyCantBeRead : Exception
    {
        /// <summary>
        /// DuckType property can't be read
        /// </summary>
        public DuckTypePropertyCantBeRead() : base("The property can't be read, you should remove the getter from the interface."){}
    }
    /// <summary>
    /// DuckType property can't be written
    /// </summary>
    public class DuckTypePropertyCantBeWritten : Exception
    {
        /// <summary>
        /// DuckType property can't be written
        /// </summary>
        public DuckTypePropertyCantBeWritten() : base("The property can't be written, you should remove the setter from the interface.") {}
    }
    /// <summary>
    /// DuckType field is readonly
    /// </summary>
    public class DuckTypeFieldIsReadonly : Exception
    {
        /// <summary>
        /// DuckType field is readonly
        /// </summary>
        public DuckTypeFieldIsReadonly() : base("The field is marked as readonly, you should remove the setter from the interface.") {}
    }
    /// <summary>
    /// DuckType property or field not found
    /// </summary>
    public class DuckTypePropertyOrFieldNotFound : Exception
    {
        /// <summary>
        /// DuckType property or field not found
        /// </summary>
        public DuckTypePropertyOrFieldNotFound() : base("The property or field was not found in the instance.") {}
    }
}