using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Expression Tree Fetcher
    /// </summary>
    public sealed class ExpressionTreeFetcher: Fetcher
    {
        private readonly Func<object, object> _getFunc;
        private readonly Action<object, object> _setFunc;
        
        /// <summary>
        /// Creates a new fetcher for a property
        /// </summary>
        /// <param name="property">Property info</param>
        public ExpressionTreeFetcher(PropertyInfo property) : base(property.Name)
        {
            Type = FetcherType.Property;
            _getFunc = property.CanRead ? BuildGetAccessor(property) : (obj) => null!;
            _setFunc = property.CanWrite ? BuildSetAccessor(property) : (obj, val) => {};
        }
        
        /// <summary>
        /// Creates a new fetcher for a field
        /// </summary>
        /// <param name="field">Field info</param>
        public ExpressionTreeFetcher(FieldInfo field) : base(field.Name)
        {
            Type = FetcherType.Field;
            _getFunc = BuildGetAccessor(field);
            _setFunc = (field.Attributes & FieldAttributes.InitOnly) == 0 ? BuildSetAccessor(field) : (obj, val) => {};
        }

        /// <summary>
        /// Fetch value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <returns>Value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object? Fetch(object? obj) => _getFunc(obj!);

        /// <summary>
        /// Shove value
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="value">Value</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void Shove(object? obj, object? value) => _setFunc(obj!, value!);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<object, object> BuildGetAccessor(PropertyInfo property)
        {
            var method = property.GetMethod;
            var obj = Expression.Parameter(typeof(object), "obj");
            MethodCallExpression call;
            if (method.IsStatic)
            {
                call = Expression.Call(method);
            }
            else
            {
                var instance = Expression.Convert(obj, method.DeclaringType);
                call = Expression.Call(instance, method);
            }
            var result = Expression.Convert(call, typeof(object));
            var expr = Expression.Lambda<Func<object, object>>(result, "GetProp+" + property.Name, new[] { obj });
            return expr.Compile();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Func<object, object> BuildGetAccessor(FieldInfo field)
        {
            var obj = Expression.Parameter(typeof(object), "obj");
            MemberExpression call;
            if (field.IsStatic)
            {
                call = Expression.Field(null, field);
            }
            else
            {
                var instance = Expression.Convert(obj, field.DeclaringType);
                call = Expression.Field(instance, field);
            }
            var result = Expression.Convert(call, typeof(object));
            var expr = Expression.Lambda<Func<object, object>>(result, "GetField+" + field.Name, new[] { obj });
            return expr.Compile();
        }
        
        /// <summary>
        /// Build a set accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Action<object, object> BuildSetAccessor(PropertyInfo property)
        {
            var method = property.SetMethod;
            var obj = Expression.Parameter(typeof(object), "obj");
            var value = Expression.Parameter(typeof(object), "value");
            MethodCallExpression call;
            if (method.IsStatic)
            {
                var castedValue = Expression.Convert(value, method.GetParameters()[0].ParameterType);
                call = Expression.Call(method, castedValue);
            }
            else
            {
                var instance = Expression.Convert(obj, method.DeclaringType);
                var castedValue = Expression.Convert(value, method.GetParameters()[0].ParameterType);
                call = Expression.Call(instance, method, castedValue);
            }
            var expr = Expression.Lambda<Action<object, object>>(call, "SetProp+" + property.Name, new[] { obj, value });
            return expr.Compile();
        }
        
        /// <summary>
        /// Build a set accessor from a field info
        /// </summary>
        /// <param name="field">Field info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Action<object, object> BuildSetAccessor(FieldInfo field)
        {
            var obj = Expression.Parameter(typeof(object), "obj");
            var value = Expression.Parameter(typeof(object), "value");
            Expression call;
            if (field.IsStatic)
            {
                var castedValue = Expression.Convert(value, field.FieldType);
                call = Expression.Assign(Expression.Field(null, field), castedValue);
            }
            else
            {
                var instance = Expression.Convert(obj, field.DeclaringType);
                var castedValue = Expression.Convert(value, field.FieldType);
                call = Expression.Assign(Expression.Field(instance, field), castedValue);
            }
            var expr = Expression.Lambda<Action<object, object>>(call, "SetField+" + field.Name, new[] { obj, value });
            return expr.Compile();
        }
    }
}