using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
// ReSharper disable InconsistentNaming

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Internal IL Helpers
    /// </summary>
    internal static class ILHelpers
    {
        private static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod("GetTypeFromHandle")!;
        private static readonly MethodInfo ConvertTypeMethodInfo = typeof(Util).GetMethod("ConvertType")!;

        /// <summary>
        /// Conversion OpCodes
        /// </summary>
        private static readonly Dictionary<Type, OpCode> ConvOpCodes = new Dictionary<Type,OpCode>
        {
            {typeof(sbyte),  OpCodes.Conv_I1},
            {typeof(short),  OpCodes.Conv_I2},
            {typeof(int),  OpCodes.Conv_I4},
            {typeof(long),  OpCodes.Conv_I8},
            
            {typeof(byte),  OpCodes.Conv_U1},
            {typeof(ushort),  OpCodes.Conv_U2},
            {typeof(uint),  OpCodes.Conv_U4},
            {typeof(ulong),  OpCodes.Conv_I8},
            
            {typeof(char),  OpCodes.Conv_U2},
            {typeof(float),  OpCodes.Conv_R4},
            {typeof(double),  OpCodes.Conv_R8},
        };
        
        /// <summary>
        /// Load instance field
        /// </summary>
        /// <param name="il">IlGenerator</param>
        /// <param name="instanceField">Instance field</param>
        /// <param name="instanceType">Instance type</param>
        internal static void LoadInstance(ILGenerator il, FieldInfo instanceField, Type instanceType)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            if (!instanceType.IsPublic && !instanceType.IsNestedPublic) return;
            if (instanceType.IsValueType)
            {
                il.DeclareLocal(instanceType);
                il.Emit(OpCodes.Unbox_Any, instanceType);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, 0);
            }
            else if (instanceType != typeof(object))
            {
                il.Emit(OpCodes.Castclass, instanceType);
            }
        }

        /// <summary>
        /// Load instance argument
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="actualType">Actual type</param>
        /// <param name="expectedType">Expected type</param>
        internal static void LoadInstanceArgument(ILGenerator il, Type actualType, Type expectedType)
        {
            il.Emit(OpCodes.Ldarg_0);
            if (actualType == expectedType) return;
            if (expectedType.IsValueType)
            {
                il.DeclareLocal(expectedType);
                il.Emit(OpCodes.Unbox_Any, expectedType);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, 0);
            }
            else
            {
                il.Emit(OpCodes.Castclass, expectedType);
            }
        }
        
        /// <summary>
        /// Write load arguments
        /// </summary>
        /// <param name="index">Argument index</param>
        /// <param name="il">IlGenerator</param>
        /// <param name="isStatic">Define if we need to take into account the instance argument</param>
        internal static void WriteLoadArgument(int index, ILGenerator il, bool isStatic)
        {
            switch (index)
            {
                case 0:
                    il.Emit(isStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
                    break;
                case 1:
                    il.Emit(isStatic ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2);
                    break;
                case 2:
                    il.Emit(isStatic ? OpCodes.Ldarg_2 : OpCodes.Ldarg_3);
                    break;
                case 3:
                    if (isStatic)
                        il.Emit(OpCodes.Ldarg_3);
                    else
                        il.Emit(OpCodes.Ldarg_S, 4);
                    break;
                default:
                    il.Emit(OpCodes.Ldarg_S, isStatic ? index : index + 1);
                    break;
            }
        }

        /// <summary>
        /// Write int value
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="value">Integer value</param>
        internal static void WriteIlIntValue(ILGenerator il, int value)
        {
            switch (value)
            {
                case 0: 
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    il.Emit(OpCodes.Ldc_I4_S, value);
                    break;
            }
        }
        
        /// <summary>
        /// Convert a current type to an expected type
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="actualType">Actual type</param>
        /// <param name="expectedType">Expected type</param>
        internal static void TypeConversion(ILGenerator il, Type actualType, Type expectedType)
        {
            if (actualType == expectedType) return;
            var actualUnderlyingType = actualType.IsEnum ? Enum.GetUnderlyingType(actualType) : actualType;
            var expectedUnderlyingType = expectedType.IsEnum ? Enum.GetUnderlyingType(expectedType) : expectedType;
            
            if (actualUnderlyingType.IsValueType)
            {
                if (expectedUnderlyingType.IsValueType)
                {
                    if (ConvertValueTypes(il, actualUnderlyingType, expectedUnderlyingType)) return;
                    il.Emit(OpCodes.Box, actualUnderlyingType);
                    il.Emit(OpCodes.Unbox_Any, expectedUnderlyingType);
                }
                else
                {
                    il.Emit(OpCodes.Box, actualType);
                    il.Emit(OpCodes.Castclass, expectedType);
                }
            }
            else
            {
                if (expectedType.IsValueType)
                {
                    il.Emit(OpCodes.Ldtoken, expectedUnderlyingType); 
                    il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null); 
                    il.EmitCall(OpCodes.Call, ConvertTypeMethodInfo, null); 
                    il.Emit(OpCodes.Unbox_Any, expectedType);
                }
                else
                {
                    il.Emit(OpCodes.Castclass, expectedType);
                }
            }
        }

        /// <summary>
        /// Converts basic value types using the conversion OpCodes
        /// </summary>
        /// <param name="il">ILGenerator</param>
        /// <param name="currentType">Current value type</param>
        /// <param name="expectedType">Expected value type</param>
        /// <returns>True if the conversion was made; otherwise, false</returns>
        private static bool ConvertValueTypes(ILGenerator il, Type currentType, Type expectedType)
        {
            if (currentType == expectedType) return true;

            if (currentType == typeof(byte) &&
                expectedType != typeof(sbyte) && expectedType != typeof(short) &&
                expectedType != typeof(int) && expectedType != typeof(uint))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(short) && expectedType != typeof(int) && expectedType != typeof(uint))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(int) && expectedType != typeof(uint))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(long) && expectedType != typeof(ulong))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(sbyte) &&
                expectedType != typeof(short) && expectedType != typeof(int) && expectedType != typeof(uint))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }

            if (currentType == typeof(ushort) &&
                expectedType != typeof(int) && expectedType != typeof(uint) && expectedType != typeof(char))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(uint) && expectedType != typeof(int))
            {
                if (expectedType == typeof(float) || expectedType == typeof(double))
                    il.Emit(OpCodes.Conv_R_Un);
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(ulong) && expectedType != typeof(long))
            {
                if (expectedType == typeof(float) || expectedType == typeof(double))
                    il.Emit(OpCodes.Conv_R_Un);
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(char) && expectedType != typeof(int) && expectedType != typeof(uint) &&
                expectedType != typeof(ushort))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(float))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            if (currentType == typeof(double))
            {
                il.Emit(ConvOpCodes[expectedType]);
                return true;
            }
            return false;
        }
    }
}