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
        private static readonly ConcurrentDictionary<VTuple<Type,Type>, Type> DuckTypeCache = new ConcurrentDictionary<VTuple<Type,Type>, Type>();
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly ConcurrentBag<DynamicMethod> DynamicMethods = new ConcurrentBag<DynamicMethod>();
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo GetInnerDuckTypeMethodInfo = typeof(DuckType).GetMethod("GetInnerDuckType", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo SetInnerDuckTypeMethodInfo = typeof(DuckType).GetMethod("SetInnerDuckType", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo FetchMethodInfo = typeof(DuckType).GetMethod("Fetch", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo ShoveMethodInfo = typeof(DuckType).GetMethod("Shove", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly MethodInfo InvokeMethodInfo = typeof(DuckType).GetMethod("Invoke", BindingFlags.Static | BindingFlags.NonPublic);
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)] 
        private static readonly ConcurrentDictionary<VTuple<string, TypeBuilder>, FieldInfo> DynamicFields = new ConcurrentDictionary<VTuple<string, TypeBuilder>, FieldInfo>();
        
        #region Fields
        
        /// <summary>
        /// Current instance
        /// </summary>
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        protected object? CurrentInstance;
        /// <summary>
        /// Instance type
        /// </summary>
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        private Type? _type;
        /// <summary>
        /// Assembly version
        /// </summary>
        [DebuggerBrowsableAttribute(DebuggerBrowsableState.Never)]
        private Version? _version;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Instance
        /// </summary>
        public object? Instance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CurrentInstance;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal set => CurrentInstance = value;
        }

        /// <summary>
        /// Instance Type
        /// </summary>
        public Type? Type => _type ??= CurrentInstance?.GetType();

        /// <summary>
        /// Assembly version
        /// </summary>
        public Version? AssemblyVersion => _version ??= Type?.Assembly?.GetName().Version;
        
        #endregion    
        
        #region .ctor
        
        /// <summary>
        /// Duck type
        /// </summary>
        protected DuckType(){}
        
        #endregion
        
        #region Create
        
        /// <summary>
        /// Create duck type proxy from an interface
        /// </summary>
        /// <param name="instance">Instance object</param>
        /// <typeparam name="T">Interface type</typeparam>
        /// <returns>Duck type proxy</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Create<T>(object instance)
        {
            return (T)(object) Create(typeof(T), instance);
        }
        
        /// <summary>
        /// Create duck type proxy from an interface type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instance">Instance object</param>
        /// <returns>Duck Type proxy</returns>
        public static DuckType Create(Type interfaceType, object instance)
        {
            EnsureArguments(interfaceType, instance);

            // Create Type
            var type = GetOrCreateProxyType(interfaceType, instance.GetType()); 
            
            // Create instance
            var objInstance = (DuckType)FormatterServices.GetUninitializedObject(type);
            objInstance.Instance = instance;
            return objInstance;
        }
        
        /// <summary>
        /// Create a duck type proxy from an interface type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instanceType">Instance type</param>
        /// <returns>Duck Type proxy</returns>
        public static DuckType Create(Type interfaceType, Type instanceType)
        {
            var type = GetOrCreateProxyType(interfaceType, instanceType);
            return (DuckType) FormatterServices.GetUninitializedObject(type);
        }

        #endregion
        
        #region GetFactory
        
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <typeparam name="T">Type of interface</typeparam>
        /// <returns>Duck Type factory</returns>
        public static IDuckTypeFactory<T> GetFactory<T>(object instance) where T:class
        {
            var interfaceType = typeof(T);
            EnsureArguments(interfaceType, instance);

            // Create Type
            var type = GetOrCreateProxyType(interfaceType, instance.GetType());
            return new DuckTypeFactory<T>(type);
        }
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instanceType">Object type</param>
        /// <returns>Duck type factory</returns>
        public static IDuckTypeFactory<object> GetFactoryByTypes(Type interfaceType, Type instanceType)
        {
            var type = GetOrCreateProxyType(interfaceType, instanceType);
            return new DuckTypeFactory<object>(type);
        }
        /// <summary>
        /// Gets a ducktype factory for an interface and instance type
        /// </summary>
        /// <param name="instanceType">Type of instance</param>
        /// <typeparam name="T">Type of interface</typeparam>
        /// <returns>Duck Type factory</returns>
        public static IDuckTypeFactory<T> GetFactoryByTypes<T>(Type instanceType) where T:class
        {
            var interfaceType = typeof(T);
            var type = GetOrCreateProxyType(interfaceType, instanceType);
            return new DuckTypeFactory<T>(type);
        }

        #endregion
        
        #region Utilities Methods
        
        /// <summary>
        /// Checks and ensures the arguments for the Create methods
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instance">Instance value</param>
        /// <exception cref="ArgumentNullException">If the interface type or the instance value is null</exception>
        /// <exception cref="ArgumentException">If the interface type is not an interface or is neither public or nested public</exception>
        private static void EnsureArguments(Type interfaceType, object instance)
        {
            if (interfaceType is null)
                throw new ArgumentNullException(nameof(interfaceType), "The interface type can't be null");
            if (instance is null)
                throw new ArgumentNullException(nameof(instance), "The object instance can't be null");
            if (!interfaceType.IsInterface)
                throw new DuckTypeTypeIsNotAnInterfaceException(interfaceType, nameof(interfaceType));
            if (!interfaceType.IsPublic && !interfaceType.IsNestedPublic)
                throw new DuckTypeTypeIsNotPublicException(interfaceType, nameof(interfaceType));
        }
        
        /// <summary>
        /// Get inner DuckType
        /// </summary>
        /// <param name="field">Field reference</param>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="value">Property value</param>
        /// <returns>DuckType instance</returns>
        protected static DuckType GetInnerDuckType(ref DuckType field, Type interfaceType, object? value)
        {
            if (value is null)
            {
                field = null!;
                return field;
            }
            var valueType = value.GetType();
            if (field is null || field.Type != valueType)
                field = Create(interfaceType, valueType);
            field.Instance = value;
            return field;
        }

        /// <summary>
        /// Set inner DuckType
        /// </summary>
        /// <param name="field">Field reference</param>
        /// <param name="value">DuckType instance</param>
        /// <returns>Property value</returns>
        protected static object? SetInnerDuckType(ref DuckType field, DuckType? value)
        {
            field = value!;
            return field?.Instance;
        }

        /// <summary>
        /// Fetch from a dynamic fetcher
        /// </summary>
        /// <param name="fetcher">Dynamic Fetcher instance</param>
        /// <param name="fetcherName">Property or Field name</param>
        /// <param name="instance">Object instance</param>
        /// <returns>Fetch value</returns>
        protected static object? Fetch(ref DynamicFetcher fetcher, string fetcherName, object? instance)
        {
            if (fetcher is null)
                fetcher = new DynamicFetcher(fetcherName, DuckAttribute.AllFlags);
            return fetcher.Fetch(instance);
        }
        /// <summary>
        /// Shove to a dynamic fetcher
        /// </summary>
        /// <param name="fetcher">Dynamic Fetcher instance</param>
        /// <param name="fetcherName">Property or Field name</param>
        /// <param name="instance">Object instance</param>
        /// <param name="value">Value to shove</param>
        protected static void Shove(ref DynamicFetcher fetcher, string fetcherName, object? instance, object? value)
        {
            if (fetcher is null)
                fetcher = new DynamicFetcher(fetcherName, DuckAttribute.AllFlags);
            fetcher.Shove(instance, value);
        }
        /// <summary>
        /// Invoke a method from a dynamic fetcher
        /// </summary>
        /// <param name="fetcher">Dynamic Fetcher instance</param>
        /// <param name="fetcherName">Property or Field name</param>
        /// <param name="instance">Object instance</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns></returns>
        protected static object? Invoke(ref DynamicFetcher fetcher, string fetcherName, object? instance, object[] parameters)
        {
            if (fetcher is null)
                fetcher = new DynamicFetcher(fetcherName, DuckAttribute.AllFlags);
            return fetcher.Invoke(instance, parameters);
        }
        #endregion

        /// <summary>
        /// Get or creates a proxy type implementing the interface type to access the given instance type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instanceType">Instance type</param>
        /// <returns>Proxy type</returns>
        private static Type GetOrCreateProxyType(Type interfaceType, Type instanceType)
            => DuckTypeCache.GetOrAdd(new VTuple<Type, Type>(interfaceType, instanceType), 
                types => CreateProxyType(types.Item1, types.Item2));
        
        /// <summary>
        /// Creates a proxy type implementing the interface type to access the given instance type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instanceType">Instance type</param>
        /// <returns>Proxy type</returns>
        /// <exception cref="NullReferenceException">In case the CurrentInstance field is not found</exception>
        private static Type CreateProxyType(Type interfaceType, Type instanceType)
        {
            var typeSignature = $"{interfaceType.Name}-ProxyTo->{instanceType.Name}";
                
            //Create Type
            var an = new AssemblyName(typeSignature + "Assembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType(typeSignature, 
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | 
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                typeof(DuckType), new[] { interfaceType });
            
            // Define .ctor
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Private);
            
            // Gets the current instance field info
            var instanceField = typeBuilder.BaseType!.GetField(nameof(CurrentInstance), BindingFlags.Instance | BindingFlags.NonPublic);
            if (instanceField is null)
                throw new NullReferenceException();
            
            // Create Members
            CreateInterfaceProperties(interfaceType, instanceType, instanceField, typeBuilder);
            CreateInterfaceMethods(interfaceType, instanceType, instanceField, typeBuilder);
            
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

                var duckAttrs = new List<DuckAttribute>(iProperty.GetCustomAttributes<DuckAttribute>(true));
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

                if (iProperty.CanRead && propertyBuilder.GetMethod is null)
                    propertyBuilder.SetGetMethod(GetNotFoundGetMethod(instanceType, typeBuilder, iProperty));

                if (iProperty.CanWrite && propertyBuilder.SetMethod is null)
                    propertyBuilder.SetSetMethod(GetNotFoundSetMethod(instanceType, typeBuilder, iProperty));
            }
        }

        private static MethodBuilder GetPropertyGetMethod(Type instanceType, TypeBuilder typeBuilder, 
            PropertyInfo iProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            Type[] parameterTypes;
            var idxParams = iProperty.GetIndexParameters();
            if (idxParams.Length > 0)
            {
                parameterTypes = new Type[idxParams.Length];
                for (var i = 0; i < idxParams.Length; i++)
                    parameterTypes[i] = idxParams[i].ParameterType;
            }
            else
            {
                parameterTypes = Type.EmptyTypes;
            }
            var method = typeBuilder.DefineMethod("get_" + iProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                iProperty.PropertyType, parameterTypes);

            var il = method.GetILGenerator();

            if (prop.CanRead)
            {
                var propMethod = prop.GetMethod;

                // Check if an inner duck type is needed
                var innerDuck = false;
                var iPropTypeInterface = iProperty.PropertyType;
                if (iPropTypeInterface.IsGenericType)
                    iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
                if (iProperty.PropertyType != prop.PropertyType && idxParams.Length == 0 && iProperty.PropertyType.IsInterface && prop.PropertyType.GetInterface(iPropTypeInterface.FullName) == null)
                {
                    if (propMethod.IsStatic)
                    {
                        var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                        il.Emit(OpCodes.Ldsflda, innerField);
                    }
                    else
                    {
                        var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldflda, innerField);
                    }
                    il.Emit(OpCodes.Ldtoken, iProperty.PropertyType);
                    il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    innerDuck = true;
                }

                if (instanceType.IsPublic || instanceType.IsNestedPublic)
                {
                    // Load the instance
                    if (!propMethod.IsStatic)
                        LoadInstance(il, instanceField, instanceType);

                    // If we have index parameters we need to pass it
                    if (parameterTypes.Length > 0)
                    {
                        var propIdxParams = prop.GetIndexParameters();
                        for (var i = 0; i < parameterTypes.Length; i++)
                        {
                            WriteLoadArgument(i, il, propMethod);
                            var iPType = Util.GetRootType(parameterTypes[i]);
                            var pType = Util.GetRootType(propIdxParams[i].ParameterType);
                            TypeConversion(il, iPType, pType);
                        }
                    }
                    
                    // Method call
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
                }
                else
                {
                    // We can't access to a non public instance using IL, So we need to get the property value using a dynamic fetcher

                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dFetcher" + iProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DynamicFetcher), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                    il.Emit(OpCodes.Ldstr, prop.Name);
                    if (!propMethod.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, instanceField);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                    il.EmitCall(OpCodes.Call, FetchMethodInfo, null);
                }

                // Handle return value
                if (innerDuck)
                    il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
                else if (prop.PropertyType != iProperty.PropertyType)
                    TypeConversion(il, prop.PropertyType, iProperty.PropertyType);

                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Newobj, typeof(DuckTypePropertyCantBeReadException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }

            return method;
        }

        private static MethodBuilder GetPropertySetMethod(Type instanceType, TypeBuilder typeBuilder, 
            PropertyInfo iProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            Type[] parameterTypes;
            var idxParams = iProperty.GetIndexParameters();
            if (idxParams.Length > 0)
            {
                parameterTypes = new Type[idxParams.Length + 1];
                for (var i = 0; i < idxParams.Length; i++)
                    parameterTypes[i] = idxParams[i].ParameterType;
                parameterTypes[idxParams.Length] = iProperty.PropertyType;
            }
            else
            {
                parameterTypes = new[] {iProperty.PropertyType};
            }
            var method = typeBuilder.DefineMethod("set_" + iProperty.Name, 
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                typeof(void), 
                parameterTypes);

            var il = method.GetILGenerator();
                        
            if (prop.CanWrite)
            {
                var propMethod = prop.SetMethod;

                if (instanceType.IsPublic || instanceType.IsNestedPublic)
                {
                    // Load instance
                    if (!propMethod.IsStatic)
                        LoadInstance(il, instanceField, instanceType);
                }
                else
                {
                    // We can't access to a non public instance using IL, So we need to set the property value using a dynamic fetcher
                    
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dFetcher" + iProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DynamicFetcher), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                    il.Emit(OpCodes.Ldstr, prop.Name);
                    if (!propMethod.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, instanceField);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                }

                // Check if a duck type object
                var iPropTypeInterface = iProperty.PropertyType;
                if (iPropTypeInterface.IsGenericType)
                    iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
                if (iProperty.PropertyType != prop.PropertyType && idxParams.Length == 0 && iProperty.PropertyType.IsInterface && prop.PropertyType.GetInterface(iPropTypeInterface.FullName) == null)
                {
                    if (propMethod.IsStatic)
                    {
                        var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                        il.Emit(OpCodes.Ldsflda, innerField);
                    }
                    else
                    {
                        var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldflda, innerField);
                    }
                    // Load value
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, typeof(DuckType));
                    il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
                }
                else
                {
                    // Load values
                    // If we have index parameters we need to pass it
                    Type[] propTypes;
                    var idxPropParams = prop.GetIndexParameters();
                    if (idxPropParams.Length > 0)
                    {
                        propTypes = new Type[idxPropParams.Length + 1];
                        for (var i = 0; i < idxPropParams.Length; i++)
                            propTypes[i] = idxPropParams[i].ParameterType;
                        propTypes[idxParams.Length] = prop.PropertyType;
                    }
                    else
                    {
                        propTypes = new[] {prop.PropertyType};
                    }
                    for (var i = 0; i < parameterTypes.Length; i++)
                    {
                        WriteLoadArgument(i, il, propMethod);
                        var propRootType = Util.GetRootType(propTypes[i]);
                        var iPropRootType = Util.GetRootType(parameterTypes[i]);
                        TypeConversion(il, iPropRootType, propRootType);
                    }
                }

                if (instanceType.IsPublic || instanceType.IsNestedPublic)
                {
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
                }
                else
                {
                    // We can't access to a non public instance using IL, So we need to set the property value using a dynamic fetcher
                    
                    il.EmitCall(OpCodes.Call, ShoveMethodInfo, null);
                }

                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Newobj, typeof(DuckTypePropertyCantBeWrittenException).GetConstructor(Type.EmptyTypes)!);
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
            var iPropTypeInterface = iProperty.PropertyType;
            if (iPropTypeInterface.IsGenericType)
                iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
            if (iProperty.PropertyType != field.FieldType && iProperty.PropertyType.IsInterface && field.FieldType.GetInterface(iPropTypeInterface.FullName) == null)
            {
                if (field.IsStatic)
                {
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + iProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                    il.Emit(OpCodes.Ldsflda, innerField);
                }
                else
                {
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + iProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                }
                il.Emit(OpCodes.Ldtoken, iProperty.PropertyType);
                il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                innerDuck = true;
            }

            if (instanceType.IsPublic || instanceType.IsNestedPublic)
            {
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
                    var handle = (RuntimeMethodHandle) getMethodDescriptorInfo!.Invoke(getMethod, null);

                    il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, getMethod.CallingConvention,
                        getMethod.ReturnType,
                        getMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                        null);
                    DynamicMethods.Add(getMethod);
                }
            }
            else
            {
                // We can't access to a non public instance using IL, So we need to get the field value using a dynamic fetcher
                var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dFetcher" + iProperty.Name, typeBuilder), tuple =>
                    tuple.Item2.DefineField(tuple.Item1, typeof(DynamicFetcher), FieldAttributes.Private));
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldflda, innerField);
                il.Emit(OpCodes.Ldstr, field.Name);
                if (!field.IsStatic)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, instanceField);
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                }
                il.EmitCall(OpCodes.Call, FetchMethodInfo, null);
            }

            if (innerDuck)
                il.EmitCall(OpCodes.Call, GetInnerDuckTypeMethodInfo, null);
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
                il.Emit(OpCodes.Newobj, typeof(DuckTypeFieldIsReadonlyException).GetConstructor(Type.EmptyTypes)!);
                il.Emit(OpCodes.Throw);
            }
            else
            {
                if (instanceType.IsPublic || instanceType.IsNestedPublic)
                {
                    // Load instance
                    if (!field.IsStatic)
                        LoadInstance(il, instanceField, instanceType);
                }
                else
                {
                    // We can't access to a non public instance using IL, So we need to set the field value using a dynamic fetcher
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dFetcher" + iProperty.Name, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DynamicFetcher), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                    il.Emit(OpCodes.Ldstr, field.Name);
                    if (!field.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, instanceField);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                }

                // Check if a duck type object
                var iPropTypeInterface = iProperty.PropertyType;
                if (iPropTypeInterface.IsGenericType)
                    iPropTypeInterface = iPropTypeInterface.GetGenericTypeDefinition();
                if (iProperty.PropertyType != field.FieldType && iProperty.PropertyType.IsInterface && field.FieldType.GetInterface(iPropTypeInterface.FullName) == null)
                {
                    if (field.IsStatic)
                    {
                        var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dtStatic" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private | FieldAttributes.Static));
                        il.Emit(OpCodes.Ldsflda, innerField);
                    }
                    else
                    {
                        var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dt" + iProperty.Name, typeBuilder), tuple =>
                            tuple.Item2.DefineField(tuple.Item1, typeof(DuckType), FieldAttributes.Private));
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldflda, innerField);
                    }
                    // Load value
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Castclass, typeof(DuckType));
                    il.EmitCall(OpCodes.Call, SetInnerDuckTypeMethodInfo, null);
                }
                else
                {
                    // Load value
                    il.Emit(OpCodes.Ldarg_1);
                }
                
                var fieldRootType = Util.GetRootType(field.FieldType);
                var iPropRootType = Util.GetRootType(iProperty.PropertyType);
                TypeConversion(il, iPropRootType, fieldRootType);

                if (instanceType.IsPublic || instanceType.IsNestedPublic)
                {
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
                        var handle = (RuntimeMethodHandle) getMethodDescriptorInfo!.Invoke(setMethod, null);

                        il.Emit(OpCodes.Ldc_I8, (long) handle.GetFunctionPointer());
                        il.Emit(OpCodes.Conv_I);
                        il.EmitCalli(OpCodes.Calli, setMethod.CallingConvention,
                            setMethod.ReturnType,
                            setMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                            null);
                        DynamicMethods.Add(setMethod);
                    }
                }
                else
                {
                    // We can't access to a non public instance using IL, So we need to set the field value using a dynamic fetcher
                    il.EmitCall(OpCodes.Call, ShoveMethodInfo, null);
                }
            }
            il.Emit(OpCodes.Ret);

            return method;
        }

        private static MethodBuilder GetNotFoundGetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo iProperty)
        {
            Type[] parameterTypes;
            var idxParams = iProperty.GetIndexParameters();
            if (idxParams.Length > 0)
            {
                parameterTypes = new Type[idxParams.Length];
                for (var i = 0; i < idxParams.Length; i++)
                    parameterTypes[i] = idxParams[i].ParameterType;
            }
            else
            {
                parameterTypes = Type.EmptyTypes;
            }
            var method = typeBuilder.DefineMethod("get_" + iProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                iProperty.PropertyType, parameterTypes);

            var il = method.GetILGenerator();
            il.Emit(OpCodes.Newobj, typeof(DuckTypePropertyOrFieldNotFoundException).GetConstructor(Type.EmptyTypes)!);
            il.Emit(OpCodes.Throw);
            return method;
        }

        private static MethodBuilder GetNotFoundSetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo iProperty)
        {
            Type[] parameterTypes;
            var idxParams = iProperty.GetIndexParameters();
            if (idxParams.Length > 0)
            {
                parameterTypes = new Type[idxParams.Length + 1];
                for (var i = 0; i < idxParams.Length; i++)
                    parameterTypes[i] = idxParams[i].ParameterType;
                parameterTypes[idxParams.Length] = iProperty.PropertyType;
            }
            else
            {
                parameterTypes = new[] {iProperty.PropertyType};
            }
            var method = typeBuilder.DefineMethod("set_" + iProperty.Name, 
                MethodAttributes.Public | MethodAttributes.SpecialName | 
                MethodAttributes.HideBySig | MethodAttributes.Virtual, 
                typeof(void), 
                parameterTypes);

            var il = method.GetILGenerator();
            il.Emit(OpCodes.Newobj, typeof(DuckTypePropertyOrFieldNotFoundException).GetConstructor(Type.EmptyTypes)!);
            il.Emit(OpCodes.Throw);
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

                var iMethodGenericArguments = iMethod.GetGenericArguments();
                var iMethodGenericNames = iMethodGenericArguments.Select((t, i) => "T" + (i + 1)).ToArray();
                if (iMethodGenericNames.Length > 0)
                {
                    var genericParameters = methodBuilder.DefineGenericParameters(iMethodGenericNames);
                }
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
                var iMethodReturnType = iMethod.ReturnType;
                if (iMethodReturnType.IsGenericType)
                    iMethodReturnType = iMethodReturnType.GetGenericTypeDefinition();
                if (iMethod.ReturnType != method.ReturnType && method.ReturnType.IsInterface && method.ReturnType.GetInterface(iMethodReturnType.FullName) == null)
                {
                    il.Emit(OpCodes.Ldtoken, iMethod.ReturnType);
                    il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    innerDuck = true;
                }

                // Create generic method call
                if (iMethodGenericArguments.Length > 0)
                    method = method.MakeGenericMethod(iMethodGenericArguments);

                if (instanceType.IsPublic || instanceType.IsNestedPublic)
                {
                    // Load instance
                    if (!method.IsStatic)
                        LoadInstance(il, instanceField, instanceType);
                    
                    // Load arguments
                    var parameters = method.GetParameters();
                    var minParametersLength = Math.Min(parameters.Length, iMethodParameters.Length);
                    for (var i = 0; i < minParametersLength; i++)
                    {
                        // Load value
                        WriteLoadArgument(i, il, iMethod);
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
                }
                else
                {
                    // We can't access to a non public instance using IL, So we need to call the method using a dynamic fetcher
                    
                    var innerField = DynamicFields.GetOrAdd(new VTuple<string, TypeBuilder>("_dFetcher" + method.MetadataToken, typeBuilder), tuple =>
                        tuple.Item2.DefineField(tuple.Item1, typeof(DynamicFetcher), FieldAttributes.Private));
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldflda, innerField);
                    il.Emit(OpCodes.Ldstr, method.Name);
                    if (!method.IsStatic)
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, instanceField);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldnull);
                    }
                    
                    // Load arguments
                    var parameters = method.GetParameters();
                    var minParametersLength = Math.Min(parameters.Length, iMethodParameters.Length);
                    WriteIlIntValue(il, minParametersLength);
                    il.Emit(OpCodes.Newarr, typeof(object));
                    for (var i = 0; i < minParametersLength; i++)
                    {
                        // Load value
                        il.Emit(OpCodes.Dup);
                        WriteIlIntValue(il, i);
                        WriteLoadArgument(i, il, iMethod);
                        var iPType = Util.GetRootType(iMethodParameters[i].ParameterType);
                        TypeConversion(il, iPType, typeof(object));
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                    il.EmitCall(OpCodes.Call, InvokeMethodInfo, null);
                    
                    // Covert return value
                    if (method.ReturnType != typeof(void)) 
                    {
                        if (innerDuck)
                            il.EmitCall(OpCodes.Call, DuckTypeCreate, null);
                        else if (iMethod.ReturnType != typeof(object))
                            TypeConversion(il, typeof(object), iMethod.ReturnType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Pop);
                    }
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
        
        
        class DuckTypeFactory<T> : IDuckTypeFactory<T>, IDuckTypeFactory where T:class
        {
            private readonly Type _proxyType;
            internal DuckTypeFactory(Type proxyType)
            {
                _proxyType = proxyType;
            }
            
            public T Create(object instance)
            {
                var inst = (DuckType) FormatterServices.GetUninitializedObject(_proxyType);
                inst.Instance = instance;
                return (inst as T)!;
            }
            public DuckTypeLeasing<T> Rent(object instance)
                => DuckTypeLeasing<T>.Rent(this, instance);
            
            DuckType IDuckTypeFactory.Create(object instance)
            {
                var inst = (DuckType) FormatterServices.GetUninitializedObject(_proxyType);
                inst.Instance = instance;
                return inst;
            }
            DuckTypeLeasing<DuckType> IDuckTypeFactory.Rent(object instance)
                => DuckTypeLeasing<DuckType>.RentDuckType(this, instance);
        }
    }
}