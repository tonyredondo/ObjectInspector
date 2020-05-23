using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public class DuckType
    {
        private static readonly MethodInfo GetTypeFromHandleMethodInfo = typeof(Type).GetMethod("GetTypeFromHandle");
        private static readonly MethodInfo DuckTypeCreate = typeof(DuckType).GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        private static readonly ConcurrentDictionary<(Type InterfaceType, Type InstanceType), Type> DuckTypeCache = new ConcurrentDictionary<(Type, Type), Type>();

        /// <summary>
        /// Current instance
        /// </summary>
        protected object? CurrentInstance;

        /// <summary>
        /// Object Instance
        /// </summary>
        public object? Instance => CurrentInstance;

        /// <summary>
        /// Duck type
        /// </summary>
        protected DuckType(){}
        
        /// <summary>
        /// Set object instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        public void SetInstance(object instance)
        {
            CurrentInstance = instance;
        }

        /// <summary>
        /// Create duck type proxy from an interface type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="instance">Instance object</param>
        /// <returns>Duck Type proxy</returns>
        public static object Create(Type interfaceType, object instance)
        {
            if (interfaceType is null)
                throw new ArgumentNullException(nameof(interfaceType), "The Interface type can't be null");
            if (instance is null)
                throw new ArgumentNullException(nameof(instance), "The Instance can't be null");

            var instanceType = instance.GetType();

            var type = DuckTypeCache.GetOrAdd((interfaceType, instanceType), types =>
            {
                var typeSignature = "ProxyTo" + types.InstanceType.Name;
                
                //Create Type
                var an = new AssemblyName(typeSignature + "Assembly");
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
                var typeBuilder = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                    typeof(DuckType), new[] { types.InterfaceType });
            
                // Define .ctor
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Private);
            
                // Create Members
                CreateInterfaceProperties(types.InterfaceType, types.InstanceType, typeBuilder);
                //CreateInterfaceMethods(types.InterfaceType, types.InstanceType, typeBuilder);
            
                // Create Type
                return typeBuilder.CreateTypeInfo()!.AsType();
            });
            
            // Create instance
            var objInstance = (DuckType)FormatterServices.GetUninitializedObject(type);
            objInstance.SetInstance(instance);
            return objInstance;
        }

        private static void CreateInterfaceProperties(Type interfaceType, Type instanceType, TypeBuilder typeBuilder)
        {
            var instanceField = typeBuilder.BaseType!.GetField(nameof(CurrentInstance), BindingFlags.Instance | BindingFlags.NonPublic);
            var interfaceProperties = interfaceType.GetProperties();
            foreach (var iProperty in interfaceProperties)
            {
                var propertyBuilder = typeBuilder.DefineProperty(iProperty.Name, PropertyAttributes.None, iProperty.PropertyType, null);

                var duckAttr = iProperty.GetCustomAttribute<DuckAttribute>(true) ?? new DuckAttribute();
                duckAttr.Name ??= iProperty.Name;

                if (duckAttr.Kind == DuckKind.Property)
                {
                    var prop = instanceType.GetProperty(duckAttr.Name, duckAttr.Flags);
                    
                    if (iProperty.CanRead)
                        propertyBuilder.SetGetMethod(GetPropertyGetMethod(instanceType, typeBuilder, iProperty, prop, instanceField));

                    if (iProperty.CanWrite)
                        propertyBuilder.SetSetMethod(GetPropertySetMethod(instanceType, typeBuilder, iProperty, prop, instanceField));
                }
                else if (duckAttr.Kind == DuckKind.Field)
                {
                    var field = instanceType.GetField(duckAttr.Name, duckAttr.Flags);

                }
            }
        }

        private static void CreateInterfaceMethods(Type interfaceType, Type instanceType, TypeBuilder typeBuilder)
        {
            var interfaceMethods = interfaceType.GetMethods();
            foreach (var iMethod in interfaceMethods)
            {
                var parameters = iMethod.GetParameters();

                var paramBuilders = new ParameterBuilder[parameters.Length];
                var methodBuilder = typeBuilder.DefineMethod(iMethod.Name, 
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                    iMethod.ReturnType, parameters.Select(p => p.ParameterType).ToArray());
                for (var j = 0; j < parameters.Length; j++)
                    paramBuilders[j] = methodBuilder.DefineParameter(j, ParameterAttributes.None, parameters[j].Name);
            }
        }

        
        private static MethodBuilder GetPropertyGetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo iProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod("get_" + iProperty.Name,
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, iProperty.PropertyType, Type.EmptyTypes);

            var il = method.GetILGenerator();

            if (prop.CanRead)
            {
                var propMethod = prop.GetMethod;

                var innerDuck = false;
                if (iProperty.PropertyType.IsInterface && instanceType.GetInterface(iProperty.PropertyType.Name) == null)
                {
                    il.Emit(OpCodes.Ldtoken, iProperty.PropertyType);
                    il.EmitCall(OpCodes.Call, GetTypeFromHandleMethodInfo, null);
                    innerDuck = true;
                }

                if (!prop.GetMethod.IsStatic)
                    LoadInstance(il, instanceField, instanceType);

                if (propMethod.IsPublic)
                {
                    il.EmitCall(prop.GetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, propMethod, null);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I8, (long) propMethod.MethodHandle.GetFunctionPointer());
                    il.Emit(OpCodes.Conv_I);
                    il.EmitCalli(OpCodes.Calli, propMethod.CallingConvention,
                        propMethod.ReturnType, propMethod.GetParameters().Select(p => p.ParameterType).ToArray(), null);
                }

                if (innerDuck)
                {
                    il.EmitCall(OpCodes.Call, DuckTypeCreate, null);
                }
                else if (prop.PropertyType != iProperty.PropertyType)
                {
                    if (prop.PropertyType.IsValueType)
                        il.Emit(OpCodes.Box, prop.PropertyType);
                    il.Emit(OpCodes.Castclass, iProperty.PropertyType);
                }

                il.Emit(OpCodes.Ret);
            }
            else
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException));
                il.Emit(OpCodes.Throw);
            }

            return method;
        }

        private static MethodBuilder GetPropertySetMethod(Type instanceType, TypeBuilder typeBuilder, PropertyInfo iProperty, PropertyInfo prop, FieldInfo instanceField)
        {
            var method = typeBuilder.DefineMethod("set_" + iProperty.Name, 
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual, typeof(void), new[]{ iProperty.PropertyType });

            var il = method.GetILGenerator();
                        
            if (prop.CanWrite)
            {
                /*
                if (!prop.GetMethod.IsStatic)
                    LoadInstance(il, instanceField, instanceType);
                il.EmitCall(prop.GetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, prop.GetMethod, null);
                if (prop.PropertyType != iProperty.PropertyType)
                {
                    if (prop.PropertyType.IsValueType)
                        il.Emit(OpCodes.Box, prop.PropertyType);
                    il.Emit(OpCodes.Castclass, iProperty.PropertyType);
                }
                il.Emit(OpCodes.Ret);
                */
            }
            else
            {
                il.Emit(OpCodes.Newobj, typeof(NotImplementedException));
                il.Emit(OpCodes.Throw);
            }
            
            return method;
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
    }
}