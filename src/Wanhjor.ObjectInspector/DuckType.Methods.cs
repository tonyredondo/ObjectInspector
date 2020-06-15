using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Wanhjor.ObjectInspector
{
    public partial class DuckType
    {
        private static void CreateInterfaceMethods(Type interfaceType, Type instanceType, FieldInfo instanceField, TypeBuilder typeBuilder)
        {
            var interfaceMethods = new List<MethodInfo>(interfaceType.GetMethods().Where(m => !m.IsSpecialName));
            var implementedInterfaces = interfaceType.GetInterfaces();
            foreach (var imInterface in implementedInterfaces)
            {
                if (imInterface == typeof(IDuckType)) continue;
                var newMethods = imInterface.GetMethods()
                    .Where(m => !m.IsSpecialName && interfaceMethods.All(i => i.ToString() != m.ToString()));
                interfaceMethods.AddRange(newMethods);
            }
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
                var publicInstance = instanceType.IsPublic || instanceType.IsNestedPublic;

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

                if (publicInstance)
                {
                    // Load instance
                    if (!method.IsStatic)
                        ILHelpers.LoadInstance(il, instanceField, instanceType);
                    
                    // Load arguments
                    var parameters = method.GetParameters();
                    var minParametersLength = Math.Min(parameters.Length, iMethodParameters.Length);
                    for (var i = 0; i < minParametersLength; i++)
                    {
                        // Load value
                        ILHelpers.WriteLoadArgument(i, il, iMethod.IsStatic);
                        var iPType = Util.GetRootType(iMethodParameters[i].ParameterType);
                        var pType = Util.GetRootType(parameters[i].ParameterType);
                        ILHelpers.TypeConversion(il, iPType, pType);
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
                            ILHelpers.TypeConversion(il, method.ReturnType, iMethod.ReturnType);
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
                    ILHelpers.WriteIlIntValue(il, minParametersLength);
                    il.Emit(OpCodes.Newarr, typeof(object));
                    for (var i = 0; i < minParametersLength; i++)
                    {
                        // Load value
                        il.Emit(OpCodes.Dup);
                        ILHelpers.WriteIlIntValue(il, i);
                        ILHelpers.WriteLoadArgument(i, il, iMethod.IsStatic);
                        var iPType = Util.GetRootType(iMethodParameters[i].ParameterType);
                        ILHelpers.TypeConversion(il, iPType, typeof(object));
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                    il.EmitCall(OpCodes.Call, InvokeMethodInfo, null);
                    
                    // Covert return value
                    if (method.ReturnType != typeof(void)) 
                    {
                        if (innerDuck)
                            il.EmitCall(OpCodes.Call, DuckTypeCreate, null);
                        else if (iMethod.ReturnType != typeof(object))
                            ILHelpers.TypeConversion(il, typeof(object), iMethod.ReturnType);
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
    }
}