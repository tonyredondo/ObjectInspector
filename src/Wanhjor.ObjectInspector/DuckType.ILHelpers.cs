using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Wanhjor.ObjectInspector
{
    public partial class DuckType
    {
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
        
        
        private static void LoadInstance(ILGenerator il, FieldInfo instanceField, Type instanceType)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            if (instanceType.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, instanceType);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloca_S, 0);
            }
            else if (instanceType != typeof(object))
            {
                il.Emit(OpCodes.Castclass, instanceType);
            }
        }
        
        private static void WriteLoadArgument(int index, ILGenerator il, MethodInfo iMethod)
        {
            switch (index)
            {
                case 0:
                    il.Emit(iMethod.IsStatic ? OpCodes.Ldarg_0 : OpCodes.Ldarg_1);
                    break;
                case 1:
                    il.Emit(iMethod.IsStatic ? OpCodes.Ldarg_1 : OpCodes.Ldarg_2);
                    break;
                case 2:
                    il.Emit(iMethod.IsStatic ? OpCodes.Ldarg_2 : OpCodes.Ldarg_3);
                    break;
                case 3:
                    if (iMethod.IsStatic)
                        il.Emit(OpCodes.Ldarg_3);
                    else
                        il.Emit(OpCodes.Ldarg_S, 4);
                    break;
                default:
                    il.Emit(OpCodes.Ldarg_S, iMethod.IsStatic ? index : index + 1);
                    break;
            }
        }

        private static void TypeConversion(ILGenerator il, Type actualType, Type expectedType)
        {
            if (actualType == expectedType) return;
            
            if (actualType.IsEnum)
                actualType = Enum.GetUnderlyingType(actualType);
            if (expectedType.IsEnum)
                expectedType = Enum.GetUnderlyingType(expectedType);

            if (actualType.IsValueType)
            {
                if (expectedType.IsValueType)
                {
                    if (ConvertValueTypes(il, actualType, expectedType)) return;
                    il.Emit(OpCodes.Box, actualType);
                    il.Emit(OpCodes.Unbox_Any, expectedType);
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
                    il.Emit(OpCodes.Box, actualType); 
                    il.Emit(OpCodes.Ldtoken, expectedType); 
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

        private static bool ConvertValueTypes(ILGenerator il, Type currentType, Type expectedType)
        {
            if (currentType == expectedType) return false;

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

        private static void WriteIlIntValue(ILGenerator il, int value)
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
    }
}