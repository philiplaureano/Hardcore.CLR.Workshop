using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace ILRewriter.Extensions
{
    public static class MethodDefinitionExtensions
    {
        public static TypeReference GetReturnType(this MethodReference targetMethod)
        {
            var returnType = targetMethod.ReturnType;
            var declaringType = targetMethod.DeclaringType;

            var genericInstance = declaringType as GenericInstanceType;
            var fullName = returnType.FullName ?? string.Empty;
            if (genericInstance != null && fullName.StartsWith("!") &&
                !string.IsNullOrEmpty(fullName))
            {
                var indexText = fullName.Where(char.IsDigit).ToArray();
                var indexValue = int.Parse(new string(indexText));

                var genericArgument = genericInstance.GenericArguments[indexValue];

                var originalReturnType = returnType;
                returnType = originalReturnType.IsArray ? genericArgument.MakeArrayType() : genericArgument;
            }
            return returnType;
        }

        public static ILProcessor GetILGenerator(this MethodDefinition method)
        {
            return method.Body.GetILProcessor();
        }

        public static VariableDefinition AddLocal(this MethodDefinition methodDef, Type localType)
        {
            var declaringType = methodDef.DeclaringType;
            var module = declaringType.Module;
            var variableType = module.Import(localType);
            var result = new VariableDefinition(variableType);

            methodDef.Body.Variables.Add(result);

            return result;
        }

        public static VariableDefinition AddLocal(this MethodDefinition method, string variableName, Type variableType)
        {
            var module = method.DeclaringType.Module;
            var localType = module.Import(variableType);

            VariableDefinition newLocal = null;
            foreach (var local in method.Body.Variables)
            {
                // Match the variable name and type
                if (local.Name != variableName || local.VariableType != localType)
                    continue;

                newLocal = local;
            }

            // If necessary, create the local variable
            if (newLocal == null)
            {
                var body = method.Body;
                var index = body.Variables.Count;

                newLocal = new VariableDefinition(variableName, localType);

                body.Variables.Add(newLocal);
            }

            return newLocal;
        }

        public static void AddParameters(this MethodDefinition method, Type[] parameterTypes)
        {
            var declaringType = method.DeclaringType;
            var module = declaringType.Module;

            // Build the parameter list
            foreach (var type in parameterTypes)
            {
                var isGeneric = type.ContainsGenericParameters && type.IsGenericType;
                var hasGenericParameter = type.HasElementType && type.GetElementType().IsGenericParameter;
                var shouldImportMethodContext = isGeneric || type.IsGenericParameter || hasGenericParameter;

                var parameterType = shouldImportMethodContext ? module.Import(type, method) : module.Import(type);

                var param = new ParameterDefinition(parameterType);
                method.Parameters.Add(param);
            }
        }


        public static void SetReturnType(this MethodDefinition method, Type returnType)
        {
            var declaringType = method.DeclaringType;
            var module = declaringType.Module;

            TypeReference actualReturnType;

            if ((returnType.ContainsGenericParameters && returnType.IsGenericType) || returnType.IsGenericParameter)
                actualReturnType = module.Import(returnType, method);
            else
                actualReturnType = module.Import(returnType);

            method.ReturnType = actualReturnType;
        }

        public static TypeReference AddGenericParameter(this MethodDefinition method, Type parameterType)
        {
            // Check if the parameter type already exists
            var matches = (from GenericParameter p in method.GenericParameters
                where p.Name == parameterType.Name
                select p).ToList();

            // Reuse the existing parameter
            if (matches.Count > 0)
                return matches[0];

            var parameter = new GenericParameter(parameterType.Name, method);
            method.GenericParameters.Add(parameter);

            return parameter;
        }

        public static VariableDefinition AddLocal<T>(this MethodDefinition methodDef)
        {
            return methodDef.AddLocal(typeof(T));
        }

        public static VariableDefinition AddLocal<T>(this MethodDefinition methodDef, string variableName)
        {
            return methodDef.AddLocal(variableName, typeof(T));
        }
    }
}