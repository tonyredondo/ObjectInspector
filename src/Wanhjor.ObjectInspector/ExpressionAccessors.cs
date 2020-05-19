using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Expression trees for accessors
    /// </summary>
    internal static class ExpressionAccessors
    {
        /// <summary>
        /// Build a get accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object> BuildGetAccessor(PropertyInfo property)
        {
            var obj = Expression.Parameter(typeof(object), "obj");
            MemberExpression call;
            if (property.GetMethod.IsStatic)
            {
                call = Expression.Property(null, property);
            }
            else
            {
                var instance = Expression.Convert(obj, property.DeclaringType);
                call = Expression.Property(instance, property);
            }
            var result = Expression.Convert(call, typeof(object));
            var expr = Expression.Lambda<Func<object, object>>(result, "GetProp+" + property.Name, new[] { obj });
            return expr.Compile();
        }
        
        /// <summary>
        /// Build a set accessor from a property info
        /// </summary>
        /// <param name="property">Property info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<object, object> BuildSetAccessor(PropertyInfo property)
        {
            var obj = Expression.Parameter(typeof(object), "obj");
            var value = Expression.Parameter(typeof(object), "value");
            Expression call;
            if (property.SetMethod.IsStatic)
            {
                var castedValue = Expression.Convert(value, property.PropertyType);
                call = Expression.Assign(Expression.Property(null, property), castedValue);
            }
            else
            {
                var instance = Expression.Convert(obj, property.DeclaringType);
                var castedValue = Expression.Convert(value, property.PropertyType);
                call = Expression.Assign(Expression.Property(instance, property), castedValue);
            }
            var expr = Expression.Lambda<Action<object, object>>(call, "SetProp+" + property.Name, new[] { obj, value });
            return expr.Compile();
        }

        /// <summary>
        /// Build a get accessor from a field info
        /// </summary>
        /// <param name="field">Field info</param>
        /// <returns>Delegate to the get accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object> BuildGetAccessor(FieldInfo field)
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
        /// Build a set accessor from a field info
        /// </summary>
        /// <param name="field">Field info</param>
        /// <returns>Delegate to the set accessor</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Action<object, object> BuildSetAccessor(FieldInfo field)
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