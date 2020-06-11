using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public partial class DuckType : IDuckType
    {
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
            var interfaceProperties = new List<PropertyInfo>(interfaceType.GetProperties());
            var implementedInterfaces = interfaceType.GetInterfaces();
            foreach (var imInterface in implementedInterfaces)
            {
                if (imInterface == typeof(IDuckType)) continue;
                var newProps = imInterface.GetProperties().Where(p => interfaceProperties.All(i => i.Name != p.Name));
                interfaceProperties.AddRange(newProps);
            }
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
    }
}