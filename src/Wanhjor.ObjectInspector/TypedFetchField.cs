using System.Reflection;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Fetcher for Fields
    /// </summary>
    /// <typeparam name="TObject">Object type</typeparam>
    /// <typeparam name="TField">Field type</typeparam>
    class TypedFetchField<TObject, TField> : Fetcher
    {
        private readonly FieldInfo _field;

        /// <summary>
        /// Creates a new fetcher for a field
        /// </summary>
        /// <param name="field">Field info</param>
        public TypedFetchField(FieldInfo field) : base(field.Name)
        {
            _field = field;
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        public override object Fetch(object obj) => _field.GetValue(obj);

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        public override void Shove(object obj, object value)
        {
            if ((_field.Attributes & FieldAttributes.InitOnly) != 0)
                return;
            _field.SetValue(obj, value);
        }
    }
}
