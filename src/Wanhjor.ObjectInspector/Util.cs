using System;
using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Utilities class
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Changes a value to an expected type
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="conversionType">Expected type</param>
        /// <returns>Value with the new type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ChangeType(object value, Type conversionType)
        {
            if (value is null)
                return null!;
            conversionType = GetRootType(conversionType);
            if (conversionType.IsEnum)
                return Enum.ToObject(conversionType, value);
            return value is IConvertible ? Convert.ChangeType(value, conversionType, CultureInfo.CurrentCulture) : value;
        }

        /// <summary>
        /// Gets the root type
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>Root type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetRootType(Type type)
        {
            while (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            return type;
        }
        
        /// <summary>
        /// Convert a value to an expected type
        /// </summary>
        /// <param name="value">Current value</param>
        /// <param name="conversionType">Expected type</param>
        /// <returns>Value with the new type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ConvertType(object value, Type conversionType) 
            => value is IConvertible && value.GetType() != conversionType ? Convert.ChangeType(value, conversionType, CultureInfo.CurrentCulture) : value;
    }
}