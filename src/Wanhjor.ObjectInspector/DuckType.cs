using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Duck Type
    /// </summary>
    public abstract class DuckType
    {
        /// <summary>
        /// Object Instance
        /// </summary>
        public object? Instance { get; set; }

        /// <summary>
        /// Create duck type proxy from an interface type
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <returns>Duck Type proxy</returns>
        public static DuckType Create(Type interfaceType)
        {
            if (interfaceType is null)
                throw new ArgumentNullException(nameof(interfaceType), "The Interface type can't be null");
            
            var typeSignature = $"{interfaceType.Name}Proxy";
            
            // Create Type
            var assemblyName = new AssemblyName(typeSignature + "Assembly");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var typeBuilder = moduleBuilder.DefineType(typeSignature, 
                TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout,
                typeof(DuckType), new[] { interfaceType });
            
            // Define .ctor
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            
            // Create Members
            
            // Create Type instance
            var type = typeBuilder.CreateTypeInfo()!.AsType();
            var objType = (DuckType)Activator.CreateInstance(type);
            return objType;
        }
    }
}