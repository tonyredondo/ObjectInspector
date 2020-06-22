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
    public partial class DuckType : ISettableDuckType
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
        public object? Instance => CurrentInstance;

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
        
        void ISettableDuckType.SetInstance(object? instance)
        {
            CurrentInstance = instance;
        }

        private void SetInstance(object? instance)
        {
            CurrentInstance = instance;
        }
        
        /// <summary>
        /// Get or creates a proxy type implementing the interface type to access the given instance type
        /// </summary>
        /// <param name="duckType">Duck type</param>
        /// <param name="instanceType">Instance type</param>
        /// <returns>Proxy type</returns>
        private static Type GetOrCreateProxyType(Type duckType, Type instanceType)
            => DuckTypeCache.GetOrAdd(new VTuple<Type, Type>(duckType, instanceType), 
                types => CreateProxyType(types.Item1, types.Item2));
        
        /// <summary>
        /// Creates a proxy type implementing the interface type to access the given instance type
        /// </summary>
        /// <param name="duckType">Duck type</param>
        /// <param name="instanceType">Instance type</param>
        /// <returns>Proxy type</returns>
        /// <exception cref="NullReferenceException">In case the CurrentInstance field is not found</exception>
        private static Type CreateProxyType(Type duckType, Type instanceType)
        {
            var typeSignature = $"{duckType.Name}-ProxyTo->{instanceType.Name}";
            
            // Define parent type, interface types
            Type parentType;
            Type[] interfaceTypes;
            if (duckType.IsInterface)
            {
                parentType = typeof(DuckType);
                interfaceTypes = new[] {duckType};
            }
            else
            {
                parentType = duckType;
                interfaceTypes = Type.EmptyTypes;
            }
            
            // Gets the current instance field info
            var instanceField = parentType.GetField(nameof(CurrentInstance), BindingFlags.Instance | BindingFlags.NonPublic);
            if (instanceField is null)
                interfaceTypes = new[] { typeof(ISettableDuckType)};
            
            // Create Type
            var an = new AssemblyName(typeSignature + "Assembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType(typeSignature, 
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | 
                TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout | TypeAttributes.Sealed,
                parentType, interfaceTypes);
            
            // Define .ctor
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Private);

            // Create instance field if is null
            instanceField ??= CreateInstanceField(typeBuilder);

            // Create Members
            CreateProperties(duckType, instanceType, instanceField, typeBuilder);
            CreateMethods(duckType, instanceType, instanceField, typeBuilder);
            
            // Create Type
            return typeBuilder.CreateTypeInfo()!.AsType();
        }

        private static FieldInfo CreateInstanceField(TypeBuilder typeBuilder)
        {
            var instanceField = typeBuilder.DefineField(nameof(CurrentInstance), typeof(object), FieldAttributes.Family);

            var setInstance = typeBuilder.DefineMethod("SetInstance",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.HideBySig | MethodAttributes.NewSlot, typeof(void), new[] {typeof(object)});
            var il = setInstance.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, instanceField);
            il.Emit(OpCodes.Ret);

            var propInstance = typeBuilder.DefineProperty("Instance", PropertyAttributes.None, typeof(object), null);
            var getPropInstance = typeBuilder.DefineMethod("get_Instance",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.HideBySig | MethodAttributes.NewSlot, typeof(object), Type.EmptyTypes);
            il = getPropInstance.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.Emit(OpCodes.Ret);
            propInstance.SetGetMethod(getPropInstance);

            var propType = typeBuilder.DefineProperty("Type", PropertyAttributes.None, typeof(Type), null);
            var getPropType = typeBuilder.DefineMethod("get_Type",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.HideBySig | MethodAttributes.NewSlot, typeof(Type), Type.EmptyTypes);
            il = getPropType.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.EmitCall(OpCodes.Callvirt, typeof(object).GetMethod("GetType"), null);
            il.Emit(OpCodes.Ret);
            propType.SetGetMethod(getPropType);

            var propVersion = typeBuilder.DefineProperty("AssemblyVersion", PropertyAttributes.None, typeof(Version), null);
            var getPropVersion = typeBuilder.DefineMethod("get_AssemblyVersion",
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.HideBySig | MethodAttributes.NewSlot, typeof(Version), Type.EmptyTypes);
            il = getPropVersion.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, instanceField);
            il.EmitCall(OpCodes.Call, typeof(object).GetMethod("GetType"), null);
            il.EmitCall(OpCodes.Callvirt, typeof(Type).GetProperty("Assembly").GetMethod, null);
            il.EmitCall(OpCodes.Callvirt, typeof(Assembly).GetMethod("GetName", Type.EmptyTypes), null);
            il.EmitCall(OpCodes.Callvirt, typeof(AssemblyName).GetProperty("Version").GetMethod, null);
            il.Emit(OpCodes.Ret);
            propVersion.SetGetMethod(getPropVersion);
            
            return instanceField;
        }

        private static List<PropertyInfo> GetProperties(Type baseType)
        {
            var selectedProperties = new List<PropertyInfo>(baseType.IsInterface ? baseType.GetProperties() : GetBaseProperties(baseType));
            var implementedInterfaces = baseType.GetInterfaces();
            foreach (var imInterface in implementedInterfaces)
            {
                if (imInterface == typeof(IDuckType)) continue;
                var newProps = imInterface.GetProperties().Where(p => selectedProperties.All(i => i.Name != p.Name));
                selectedProperties.AddRange(newProps);
            }
            return selectedProperties;
            static IEnumerable<PropertyInfo> GetBaseProperties(Type baseType)
            {
                foreach (var prop in baseType.GetProperties())
                {
                    if (prop.DeclaringType == typeof(DuckType))
                        continue;
                    if (prop.CanRead && (prop.GetMethod.IsAbstract || prop.GetMethod.IsVirtual))
                        yield return prop;
                    else if (prop.CanWrite && (prop.SetMethod.IsAbstract || prop.SetMethod.IsVirtual))
                        yield return prop;
                }
            }
        }
        
        private static void CreateProperties(Type baseType, Type instanceType, FieldInfo instanceField, TypeBuilder typeBuilder)
        {
            var asmVersion = instanceType.Assembly.GetName().Version;
            var selectedProperties = GetProperties(baseType);
            foreach (var iProperty in selectedProperties)
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