using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ILRewriter.Extensions
{
    public static class MethodBodyExtensions
    {
        public static void PackageReturnValue(this ILProcessor il, ModuleDefinition module, TypeReference returnType)
        {
            if (returnType.FullName == "System.Void")
            {
                il.Emit(OpCodes.Pop);
                return;
            }

            il.Emit(OpCodes.Unbox_Any, module.Import(returnType));
        }

        public static void PushMethod(this ILProcessor IL, MethodReference method, ModuleDefinition module)
        {
            var getMethodFromHandle = module.ImportMethod<MethodBase>("GetMethodFromHandle",
                typeof(RuntimeMethodHandle),
                typeof(RuntimeTypeHandle));

            var declaringType = method.DeclaringType;

            // Instantiate the generic type before determining
            // the current method
            if (declaringType.GenericParameters.Count > 0)
            {
                var genericType = new GenericInstanceType(declaringType);
                foreach (var parameter in declaringType.GenericParameters)
                {
                    genericType.GenericArguments.Add(parameter);
                }

                declaringType = genericType;
            }


            IL.Emit(OpCodes.Ldtoken, method);
            IL.Emit(OpCodes.Ldtoken, declaringType);
            IL.Emit(OpCodes.Call, getMethodFromHandle);
        }
    }
}
