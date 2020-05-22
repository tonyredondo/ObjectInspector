using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public abstract class DuckType
    {
        private static readonly FieldInfo InstanceFieldInfo = typeof(DuckType).GetField(nameof(_instance), BindingFlags.Instance | BindingFlags.NonPublic);
        private object? _instance;

        /// <summary>
        /// Object Instance
        /// </summary>
        public object? Instance => _instance;

        /// <summary>
        /// Set object instance
        /// </summary>
        /// <param name="instance">Object instance</param>
        public void SetInstance(object instance)
        {
            _instance = instance;
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
            
            var typeSignature = "ProxyTo" + instance.GetType().Name;
            //Create Type
            var an = new AssemblyName(typeSignature + "Assembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                typeof(DuckType), new[] { interfaceType });
            
            // Define .ctor
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            
            // Create Members
            var instanceType = instance.GetType();
            CreateInterfaceProperties(interfaceType, instanceType, typeBuilder);
            //CreateInterfaceMethods(interfaceType, instanceType, typeBuilder);
            
            // Create Type instance
            var type = typeBuilder.CreateTypeInfo()!.AsType();
            var objInstance = (DuckType)Activator.CreateInstance(type);
            objInstance.SetInstance(instance);
            return objInstance;
        }

        private static void CreateInterfaceProperties(Type interfaceType, Type instanceType, TypeBuilder typeBuilder)
        {
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
                    {
                        var propMethod = typeBuilder.DefineMethod("get_" + iProperty.Name, 
                            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, iProperty.PropertyType, Type.EmptyTypes);

                        var il = propMethod.GetILGenerator();

                        if (prop.CanRead)
                        {
                            if (!prop.GetMethod.IsStatic)
                                LoadInstance(il, instanceType);
                            il.EmitCall(prop.GetMethod.IsStatic ? OpCodes.Call : OpCodes.Callvirt, prop.GetMethod, null);
                            if (prop.PropertyType != iProperty.PropertyType)
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

                        propertyBuilder.SetGetMethod(propMethod);
                    }
                }
                else if (duckAttr.Kind == DuckKind.Field)
                {
                    var field = instanceType.GetField(duckAttr.Name, duckAttr.Flags);

                }
                
                
                if (iProperty.CanWrite)
                {
                    var method = typeBuilder.DefineMethod("set_" + iProperty.Name, 
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, 
                        null, new[] { iProperty.PropertyType });
                    
                    propertyBuilder.SetSetMethod(method);
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

        private static void LoadInstance(ILGenerator il, Type instanceType)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, InstanceFieldInfo);
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