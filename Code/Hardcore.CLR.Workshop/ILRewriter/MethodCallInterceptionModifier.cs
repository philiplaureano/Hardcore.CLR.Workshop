using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ILRewriter.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ILRewriter
{
    public class MethodCallInterceptionModifier : IAssemblyModifier
    {
        public void Modify(AssemblyDefinition assembly)
        {
            var mainModule = assembly.MainModule;
            var getInterceptorMethod = mainModule.ImportMethod("GetInterceptor", typeof(InterceptorRegistry));


            var targetType = mainModule.Types.FirstOrDefault(t => t.Name == "SampleClassWithInstanceMethod");
            if (targetType == null)
                return;

            var targetMethod = targetType.Methods.First(m => m.Name == "DoSomething");
            var methodBody = targetMethod.Body;
            var IL = methodBody.GetILProcessor();


            var currentArguments = targetMethod.AddLocal<List<object>>("__arguments");
            var currentArgument = targetMethod.AddLocal<object>("__currentArgument");
            var currentInstance = targetMethod.AddLocal<object>("__currentInstance");
            var currentMethodBase = targetMethod.AddLocal<MethodBase>("__currentMethod");

            var collectionCtor = mainModule.ImportConstructor<List<object>>();
            var addMethod = mainModule.ImportMethod("Add", typeof(List<object>));
            var toArrayMethod = mainModule.ImportMethod<List<object>>("ToArray");
            var interceptMethod = mainModule.ImportMethod<IInterceptor>("Intercept");

            var instructions = methodBody.Instructions.ToArray();

            methodBody.InitLocals = true;
            methodBody.Instructions.Clear();

            IL.Emit(OpCodes.Newobj, collectionCtor);
            IL.Emit(OpCodes.Stloc, currentArguments);

            // Create the collection that will hold the current set of arguments
            foreach (var instruction in instructions)
            {

                if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt)
                {
                    var currentMethod = (MethodReference)instruction.Operand;
                    SaveMethodCallArguments(IL, currentMethod, currentArgument, currentArguments, addMethod);

                    // Save the current object instance
                    if (!currentMethod.HasThis)
                        IL.Emit(OpCodes.Ldnull);

                    IL.Emit(OpCodes.Stloc, currentInstance);

                    
                    SaveCurrentMethod(IL, currentMethod, mainModule, currentMethodBase);

                    // Grab the inteceptor instance
                    IL.Emit(OpCodes.Call, getInterceptorMethod);

                    // Call the interceptor
                    IL.Emit(OpCodes.Ldarg_0);
                    IL.Emit(OpCodes.Ldloc, currentArguments);
                    IL.Emit(OpCodes.Callvirt, toArrayMethod);
                    IL.Emit(OpCodes.Ldloc, currentMethodBase);
                    IL.Emit(OpCodes.Callvirt, interceptMethod);

                    // Save the return value
                    IL.PackageReturnValue(mainModule, currentMethod.GetReturnType());

                    continue;
                }

                IL.Append(instruction);
            }

            //var writeLineMethod = mainModule.ImportMethod("WriteLine", typeof(Console), typeof(object));
            //IL.Emit(OpCodes.Call, writeLineMethod);
            IL.Emit(OpCodes.Ret);
        }

        private void SaveMethodCallArguments(ILProcessor il, MethodReference targetMethod,
            VariableDefinition currentArgument, VariableDefinition currentArguments,
            MethodReference pushMethod)
        {
            // If the target method is an instance method, then the remaining item on the stack
            // will be the target object instance

            // Put all the method arguments into the argument stack
            foreach (var param in targetMethod.Parameters)
            {
                // Save the current argument
                var parameterType = param.ParameterType;
                if (parameterType.IsValueType || parameterType is GenericParameter)
                    il.Emit(OpCodes.Box, parameterType);

                il.Emit(OpCodes.Stloc, currentArgument);
                il.Emit(OpCodes.Ldloc, currentArguments);
                il.Emit(OpCodes.Ldloc, currentArgument);

                il.Emit(OpCodes.Callvirt, pushMethod);
            }
        }

        private void SaveCurrentMethod(ILProcessor il, MethodReference targetMethod,
            ModuleDefinition module, VariableDefinition currentMethod)
        {
            il.PushMethod(targetMethod, module);
            il.Emit(OpCodes.Stloc, currentMethod);
        }
    }
}