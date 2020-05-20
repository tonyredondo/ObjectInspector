using System;
using System.Linq;
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
            var call = Expression.Property(property.GetMethod.IsStatic ? null : Expression.Convert(obj, property.DeclaringType), property);
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
            var call = Expression.Field(field.IsStatic ? null : Expression.Convert(obj, field.DeclaringType), field);
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
        
        /// <summary>
        /// Create an accessor delegte for a MethodInfo
        /// </summary>
        /// <param name="method">Method info instance</param>
        /// <returns>Accessor delegate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object, object[], object> BuildMethodAccessor(MethodInfo method)
        {
            var obj = Expression.Parameter(typeof(object), "o");
            Expression? castedObject = null;
            if (!method.IsStatic)
                castedObject = Expression.Convert(obj, method.DeclaringType);

            var parameters = method.GetParameters();
            var paramExp = Expression.Parameter(typeof(object[]), "args");
            var expArr = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                var pType = p.ParameterType;
                Expression argExp = Expression.ArrayIndex(paramExp, Expression.Constant(i));
                if (p.HasDefaultValue)
                {
                    argExp = Expression.Condition(Expression.Equal(argExp, Expression.Constant(null, typeof(object))),
                        Expression.Constant(p.RawDefaultValue, pType), Expression.Convert(argExp, pType));
                }
                else if (pType != typeof(object))
                {
                    argExp = Expression.Convert(Expression.Call(ChangeTypeMethodInfo, argExp, Expression.Constant(pType)), pType);
                }
                else
                {
                    argExp = Expression.Convert(argExp, pType);
                }
                expArr[i] = argExp;
            }
            Expression callExpression = Expression.Call(castedObject, method, expArr);
            if (method.ReturnType != typeof(void))
                callExpression = Expression.Convert(callExpression, typeof(object));
            else
                callExpression = Expression.Block(callExpression, Expression.Constant(null, typeof(object)));
            
            return Expression.Lambda<Func<object, object[], object>>(callExpression, "Invoker+" + method.Name, new[] { obj, paramExp }).Compile();
        }
        
        private static MethodInfo _cTypeMInfo = null!;
        private static MethodInfo ChangeTypeMethodInfo
            => _cTypeMInfo ??= typeof(ExpressionAccessors).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(n => n.Name == "ChangeType");

        private static object ChangeType(object value, Type conversionType)
        {
            if (value is IConvertible)
            {
                if (conversionType.BaseType == typeof(Enum))
                    return Enum.ToObject(conversionType, value);
                if (conversionType.IsGenericType && conversionType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return ChangeType(value, Nullable.GetUnderlyingType(conversionType)); ;
                return Convert.ChangeType(value, conversionType);
            }
            if (value is null)
                return null!;
            
            var valueType = value.GetType();
            if (conversionType.IsAssignableFrom(valueType))
                return value;
            if (conversionType.IsGenericType)
            {
                if (conversionType.GetGenericTypeDefinition() != typeof(Nullable<>))
                    throw new InvalidCastException($"The value of type: '{valueType}' can't be converted to: '{conversionType}'");
                return ChangeType(value, Nullable.GetUnderlyingType(conversionType));
            }
            return value;
        }
    }
}