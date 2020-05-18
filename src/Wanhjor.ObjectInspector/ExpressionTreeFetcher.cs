using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Expression Tree Fetcher
    /// </summary>
    internal sealed class ExpressionTreeFetcher: Fetcher
    {
        private readonly Func<object, object> _getFunc;
        
        /// <summary>
        /// Creates a new fetcher for a property
        /// </summary>
        /// <param name="property">Property info</param>
        public ExpressionTreeFetcher(PropertyInfo property) : base(property.Name)
        {
            Type = FetcherType.Property;
            _getFunc = property.CanRead ? BuildGetAccessor(property) : (obj) => null!;
        }
        
        /// <summary>
        /// Creates a new fetcher for a field
        /// </summary>
        /// <param name="field">Field info</param>
        public ExpressionTreeFetcher(FieldInfo field) : base(field.Name)
        {
            Type = FetcherType.Field;
            _getFunc = BuildGetAccessor(field);
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
        public override void Shove(object? obj, object? value)
        {
        }
    
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
    }
}