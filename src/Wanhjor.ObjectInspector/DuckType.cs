using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public class DuckType : IDuckType
    {
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod("GetTypeFromHandle")!;
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo EnumToObjectMethodInfo = typeof(Enum).GetMethod("ToObject", new[] { typeof(Type), typeof(object) })!;
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo ConvertTypeMethodInfo = typeof(Util).GetMethod("ConvertType")!;
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo DuckTypeCreate = typeof(DuckType).GetMethod("Create", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Type), typeof(object) }, null)!;
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly ConcurrentDictionary<(Type InterfaceType, Type InstanceType), Type> DuckTypeCache = new ConcurrentDictionary<(Type, Type), Type>();
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly ConcurrentBag<DynamicMethod> DynamicMethods = new ConcurrentBag<DynamicMethod>();

        /// <summary>
        /// Current instance
        /// </summary>
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        protected object? CurrentInstance;

        private Type? _type;
        private Version? _version;

        /// <summary>
        /// Instance
        /// </summary>
        public object? Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CurrentInstance;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private set => CurrentInstance = value;
        }

        /// <summary>
        /// Instance Type
        /// </summary>
        public Type? Type => _type ??= CurrentInstance?.GetType();

        /// <summary>
        /// Assembly version
        /// </summary>
        public Version? AssemblyVersion => _version ??= _type?.Assembly?.GetName().Version;
        
        /// <summary>
        /// Duck type
        /// </summary>
        protected DuckType(){}
        
        /// <summary>
        /// Create duck type proxy from an interface
        /// </summary>
        /// <param name="instance">Instance object</param>
        /// <typeparam name="T">Interface type</typeparam>
        /// <returns>Duck type proxy</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Create<T>(object instance)
        {
            return (T) Create(typeof(T), instance);
        }
        /// <summary>
        /// Create duck type proxy from an interface type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instance">Instance object</param>
        /// <returns>Duck Type proxy</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Create(Type interfaceType, object instance)
        {
            EnsureArguments(interfaceType, instance);

            // Create Type
            var type = DuckTypeCache.GetOrAdd((interfaceType, instance.GetType()), 
                types => CreateType(types));
            
            // Create instance
            var objInstance = (DuckType)FormatterServices.GetUninitializedObject(type);
            objInstance.Instance = instance;
            return objInstance;
        }

        private static void EnsureArguments(Type interfaceType, object instance)
        {
            if (interfaceType is null)
                throw new ArgumentNullException(nameof(interfaceType), "The interface type can't be null");
            if (instance is null)
                throw new ArgumentNullException(nameof(instance), "The object instance can't be null");
            if (!interfaceType.IsInterface)
                throw new ArgumentException("The type is not an interface type", nameof(interfaceType));
            if (!interfaceType.IsPublic && !interfaceType.IsNestedPublic)
                throw new ArgumentException("The interface type must be public", nameof(interfaceType));
        }

        private static Type CreateType((Type InterfaceType, Type InstanceType) types)
        {
            var typeSignature = $"{types.InterfaceType.Name}-ProxyTo->{types.InstanceType.Name}";
                
            //Create Type
            var an = new AssemblyName(typeSignature + "Assembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType(typeSignature, 
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | 
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                typeof(DuckType), new[] { types.InterfaceType });
            
            // Define .ctor
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Private);
            
            var instanceField = typeBuilder.BaseType!.GetField(nameof(CurrentInstance), BindingFlags.Instance | BindingFlags.NonPublic);
            if (instanceField is null)
                throw new NullReferenceException();
            
            // Create Members
            CreateInterfaceProperties(types.InterfaceType, types.InstanceType, instanceField, typeBuilder);
            CreateInterfaceMethods(types.InterfaceType, types.InstanceType, instanceField, typeBuilder);
            
            // Create Type
            return typeBuilder.CreateTypeInfo()!.AsType();
        }

        
        private static void CreateInterfaceProperties(Type interfaceType, Type instanceType, FieldInfo instanceField, TypeBuilder typeBuilder)
        {
            var asmVersion = instanceType.Assembly.GetName().Version;
            var interfaceProperties = interfaceType.GetProperties();
            foreach (var iProperty in interfaceProperties)
            {
                var propertyBuilder = typeBuilder.DefineProperty(iProperty.Name, PropertyAttributes.None, iProperty.PropertyType, null);

                var duckAttrs = iProperty.GetCustomAttributes<DuckAttribute>(true).ToList();
                if (duckAttrs.Count == 0)
                    duckAttrs.Add(new DuckAttribute());
                duckAttrs.Sort((x, y) =>
                {
                    if (x.Version is null) return 1;
                    if (y.Version is null) return -1;
                    return x.Version.CompareTo(y.Version);
                });

                foreach (var duckAttr in duckAttrs)
                {
                    if (!(duckAttr.Version is null) && asmVersion > duckAttr.Version)
                        continue;
                    
                    duckAttr.Name ??= iProperty.Name;

                    switch (duckAttr.Kind)
                    {
                        case DuckKind.Property:
                            var prop = instanceType.GetProperty(duckAttr.Name, duckAttr.Flags);
                            if (prop is null)
                                continue;
                    
                            if (iProperty.CanRead)
                                propertyBuilder.SetGetMethod(GetPropertyGetMethod(instanceType, typeBuilder, iProperty, prop, instanceField));

                            if (iProperty.CanWrite)
                                propertyBuilder.SetSetMethod(GetPropertySetMethod(instanceType, typeBuilder, iProperty, prop, instanceField));
                            
                            break;
                        
                        case DuckKind.Field:
                            var field = instanceType.GetField(duckAttr.Name, duckAttr.Flags);
                            if (field is null)
                                continue;

                            if (iProperty.CanRead)
                                propertyBuilder.SetGetMethod(GetFieldGetMethod(instanceType, typeBuilder, iProperty, field, instanceField));
                            
                            if (iProperty.CanWrite)
                                propertyBuilder.SetSetMethod(GetFieldSetMethod(instanceType, typeBuilder, iProperty, field, instanceField));
                            
                            break;
                    }

                    break;
                }
                
            }
        }

        private static MethodBuilder GetPropertyGetMethod(Type instanceType, TypeBuilder typeBuilder, 
            PropertyInfo iProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod("get_" + iProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                iProperty.PropertyType, Type.EmptyTypes);

            var il = method.GetILGenerator();

            if (prop.CanRead)
            {
                var propMethod = prop.GetMethod;

                var innerDuck = false;
                if (iProperty.PropertyType.IsInterface && prop.PropertyType.GetInterface(iProperty.PropertyType.FullName) == null)
                {
                    il.Emit(OpCodes.Ldtoken, iProperty.PropertyType);
                    il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    innerDuck = true;
                } 
                
                if (!propMethod.IsStatic)
                    LoadInstance(il, instanceField, instanceType);

                if (propMethod.IsPublic)
                {
                    il.EmitCall(propMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propMethod, null);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long) propMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, propMethod.CallingConvention,
                        propMethod.ReturnType, 
                        propMethod.GetParameters().Select(p => p.ParameterType).ToArray(), 
                        null);
                }

                if (innerDuck)
                    il.EmitCall(OpCodes.Call, DuckTypeCreate, null);
                else if (prop.PropertyType != iProperty.PropertyType)
                    TypeConversion(il, prop.PropertyType, iProperty.PropertyType);

                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }

            return method;
        }

        private static MethodBuilder GetPropertySetMethod(Type instanceType, TypeBuilder typeBuilder, 
            PropertyInfo iProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod("set_" + iProperty.Name, 
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                typeof(void), 
                new[]{ iProperty.PropertyType });

            var il = method.GetILGenerator();
                        
            if (prop.CanWrite)
            {
                var propMethod = prop.SetMethod;

                // Load instance
                if (!propMethod.IsStatic)
                    LoadInstance(il, instanceField, instanceType);
                
                // Load value
                il.Emit(OpCodes.Ldarg_1);
                var propRootType = Util.GetRootType(prop.PropertyType);
                var iPropRootType = Util.GetRootType(iProperty.PropertyType);
                TypeConversion(il, iPropRootType, propRootType);
                
                // Call method
                if (propMethod.IsPublic)
                {
                    il.EmitCall(propMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propMethod, null);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long) propMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, propMethod.CallingConvention,
                        propMethod.ReturnType, 
                        propMethod.GetParameters().Select(p => p.ParameterType).ToArray(), 
                        null);
                }
                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }
            
            return method;
        }

        private static MethodBuilder GetFieldGetMethod(Type instanceType, TypeBuilder typeBuilder,
            PropertyInfo iProperty, FieldInfo field, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod("get_" + iProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                iProperty.PropertyType, Type.EmptyTypes);

            var il = method.GetILGenerator();
            
            var innerDuck = false;
            if (iProperty.PropertyType.IsInterface && field.FieldType.GetInterface(iProperty.PropertyType.FullName) == null)
            {
                il.Emit(OpCodes.Ldtoken, iProperty.PropertyType);
                il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                innerDuck = true;
            }

            if (field.IsPublic)
            {
                if (field.IsStatic)
                {
                    il.Emit(OpCodes.Ldsfld, field);
                }
                else
                {
                    LoadInstance(il, instanceField, instanceType);
                    il.Emit(OpCodes.Ldfld, field);
                }
            }
            else
            {
                if (field.IsStatic)
                {
                    il.Emit(OpCodes.Ldnull);
                }
                else
                {
                    LoadInstance(il, instanceField, instanceType);
                }
                var getMethod = new DynamicMethod($"GetField+{field.DeclaringType!.Name}.{field.Name}", typeof(object), new[] {typeof(object)}, typeof(EmitAccessors).Module);
                EmitAccessors.CreateGetAccessor(getMethod.GetILGenerator(), field);

                var getMethodDescriptorInfo = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", BindingFlags.NonPublic | BindingFlags.Instance);
                var handle = (RuntimeMethodHandle)getMethodDescriptorInfo!.Invoke(getMethod, null);
                
                il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                il.Emit(OpCodes.Conv_I);
                il.EmitCalli(OpCodes.Calli, getMethod.CallingConvention,
                    getMethod.ReturnType, 
                    getMethod.GetParameters().Select(p => p.ParameterType).ToArray(), 
                    null);
                DynamicMethods.Add(getMethod);
            }
            
            if (innerDuck)
                il.EmitCall(OpCodes.Call, DuckTypeCreate, null);
            else if (field.FieldType != iProperty.PropertyType)
                TypeConversion(il, field.FieldType, iProperty.PropertyType);

            il.Emit(OpCodes.Ret);
            
            return method;
        }

        private static MethodBuilder GetFieldSetMethod(Type instanceType, TypeBuilder typeBuilder,
            PropertyInfo iProperty, FieldInfo field, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod("set_" + iProperty.Name, 
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                typeof(void), 
                new[]{ iProperty.PropertyType });

            var il = method.GetILGenerator();

            if ((field.Attributes & FieldAttributes.InitOnly) != 0)
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }
            else
            {
                // Load instance
                if (!field.IsStatic)
                    LoadInstance(il, instanceField, instanceType);

                // Load value
                il.Emit(OpCodes.Ldarg_1);
                var fieldRootType = Util.GetRootType(field.FieldType);
                var iPropRootType = Util.GetRootType(iProperty.PropertyType);
                TypeConversion(il, iPropRootType, fieldRootType);
                
                // Call method
                if (field.IsPublic)
                {
                    il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
                }
                else
                {
                    var setMethod = new DynamicMethod($"SetField+{field.DeclaringType!.Name}.{field.Name}", typeof(void), new[] {typeof(object), typeof(object)}, typeof(EmitAccessors).Module);
                    EmitAccessors.CreateSetAccessor(setMethod.GetILGenerator(), field);

                    var getMethodDescriptorInfo = typeof(DynamicMethod).GetMethod("GetMethodDescriptor", BindingFlags.NonPublic | BindingFlags.Instance);
                    var handle = (RuntimeMethodHandle)getMethodDescriptorInfo!.Invoke(setMethod, null);
                
                    il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, setMethod.CallingConvention,
                        setMethod.ReturnType, 
                        setMethod.GetParameters().Select(p => p.ParameterType).ToArray(), 
                        null);
                    DynamicMethods.Add(setMethod);
                }
            }
            il.Emit(OpCodes.Ret);

            return method;
        }
        
        
        
        private static void CreateInterfaceMethods(Type interfaceType, Type instanceType, FieldInfo instanceField, TypeBuilder typeBuilder)
        {
            var interfaceMethods = interfaceType.GetMethods().Where(m => !m.IsSpecialName);
            foreach (var iMethod in interfaceMethods)
            {
                var iMethodParameters = iMethod.GetParameters();
                var iMethodParametersTypes = iMethodParameters.Select(p => p.ParameterType).ToArray();

                var paramBuilders = new ParameterBuilder[iMethodParameters.Length];
                var methodBuilder = typeBuilder.DefineMethod(iMethod.Name, 
                    MethodAttributes.Public | MethodAttributes.Virtual | 
                    MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                    iMethod.ReturnType, iMethodParametersTypes);
                for (var j = 0; j < iMethodParameters.Length; j++)
                {
                    var cParam = iMethodParameters[j];
                    var nParam = methodBuilder.DefineParameter(j, cParam.Attributes, cParam.Name);
                    if (cParam.HasDefaultValue)
                        nParam.SetConstant(cParam.RawDefaultValue);
                    paramBuilders[j] = nParam;
                }
                var il = methodBuilder.GetILGenerator();

                // We select the method to call
                var method = SelectMethod(instanceType, iMethod, iMethodParameters, iMethodParametersTypes);

                if (method is null) 
                {
                    il.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!);
                    il.Emit(OpCodes.Throw);
                    return;
                }
                
                var innerDuck = false;
                if (method.ReturnType.IsInterface && method.ReturnType.GetInterface(iMethod.ReturnType.FullName) == null)
                {
                    il.Emit(OpCodes.Ldtoken, iMethod.ReturnType);
                    il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    innerDuck = true;
                } 
                
                // Load instance
                if (!method.IsStatic)
                    LoadInstance(il, instanceField, instanceType);
                
                // Load arguments
                var parameters = method.GetParameters();
                for (var i = 0; i < Math.Min(parameters.Length, iMethodParameters.Length); i++)
                {
                    static void WriteLoadArg(int index, ILGenerator il, MethodInfo iMethod)
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

                    // Load value
                    WriteLoadArg(i, il, iMethod);
                    var iPType = Util.GetRootType(iMethodParameters[i].ParameterType);
                    var pType = Util.GetRootType(parameters[i].ParameterType);
                    TypeConversion(il, iPType, pType);
                }
                
                // Call method
                if (method.IsPublic)
                {
                    il.EmitCall(method.IsStatic ? OpCodes.Call : OpCodes.Callvirt, method, null);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long) method.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, method.CallingConvention,
                        method.ReturnType, 
                        method.GetParameters().Select(p => p.ParameterType).ToArray(), 
                        null);
                }

                // Covert return value
                if (method.ReturnType != typeof(void)) 
                {
                    if (innerDuck)
                        il.EmitCall(OpCodes.Call, DuckTypeCreate, null);
                    else if (method.ReturnType != iMethod.ReturnType)
                        TypeConversion(il, method.ReturnType, iMethod.ReturnType);
                }

                il.Emit(OpCodes.Ret);
            }
        }

        private static MethodInfo? SelectMethod(Type instanceType, MethodInfo iMethod, ParameterInfo[] parameters, Type[] parametersTypes) 
        {
            var asmVersion = instanceType.Assembly.GetName().Version;
            var duckAttrs = iMethod.GetCustomAttributes<DuckAttribute>(true).ToList();
            if (duckAttrs.Count == 0)
                duckAttrs.Add(new DuckAttribute());
            duckAttrs.Sort((x, y) =>
            {
                if (x.Version is null) return 1;
                if (y.Version is null) return -1;
                return x.Version.CompareTo(y.Version);
            });

            MethodInfo[] allMethods = null!;
            foreach (var duckAttr in duckAttrs)
            {
                if (!(duckAttr.Version is null) && asmVersion > duckAttr.Version)
                    continue;

                duckAttr.Name ??= iMethod.Name;
                
                // We select the method to call
                var method = instanceType.GetMethod(duckAttr.Name, duckAttr.Flags, null, parametersTypes, null);
                
                if (!(method is null))
                    return method;
                
                allMethods ??= instanceType.GetMethods(duckAttr.Flags);
                        
                // Trying to select the ones with the same parameters count
                var remaining = allMethods.Where(m =>
                {
                    if (m.Name != duckAttr.Name) return false;
                    
                    var mParams = m.GetParameters();
                    if (mParams.Length == parameters.Length)
                        return true;
                    return  mParams.Count(p => p.HasDefaultValue) == parameters.Count(p => p.HasDefaultValue);
                }).ToList();

                if (remaining.Count == 0)
                    continue;
                if (remaining.Count == 1)
                    return remaining[0];
                
                // Trying to select the ones with the same return type
                var sameReturnType = remaining.Where(m => m.ReturnType == iMethod.ReturnType).ToList();
                if (sameReturnType.Count == 1)
                    return sameReturnType[0];
                    
                if (sameReturnType.Count > 1)
                    remaining = sameReturnType;

                if (iMethod.ReturnType.IsInterface && iMethod.ReturnType.GetInterface(iMethod.ReturnType.FullName) == null)
                {
                    var duckReturnType = remaining.Where(m => !m.ReturnType.IsValueType).ToList();
                    if (duckReturnType.Count == 1)
                        return duckReturnType[0];
                    
                    if (duckReturnType.Count > 1)
                        remaining = duckReturnType;
                }

                return remaining[0];
            }

            return null;
        }
        
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
    }
}