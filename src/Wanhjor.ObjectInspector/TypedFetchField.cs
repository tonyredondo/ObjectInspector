using System.Reflection;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Fetcher for Fields
    /// </summary>
    internal sealed class TypedFetchField : Fetcher
    {
        private readonly FieldInfo _field;
        private readonly bool _readOnly;

        /// <summary>
        /// Creates a new fetcher for a field
        /// </summary>
        /// <param name="field">Field info</param>
        public TypedFetchField(FieldInfo field) : base(field.Name)
        {
            _field = field;
            _readOnly = (_field.Attributes & FieldAttributes.InitOnly) != 0;
            Type = FetcherType.Field;
            
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        public override object? Fetch(object? obj) => _field.GetValue(obj);

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        public override void Shove(object? obj, object? value)
        {
            if (!_readOnly)
                _field.SetValue(obj, value);
        }
    }
}
