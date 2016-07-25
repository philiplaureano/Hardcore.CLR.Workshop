using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace ILRewriter.Extensions
{
    public static class ModuleDefinitionExtensions
    {
        public static TypeDefinition DefineClass(this ModuleDefinition mainModule,
            string typeName, string namespaceName, TypeAttributes attributes,
            TypeReference baseType)
        {
            var resultType = new TypeDefinition(typeName, namespaceName,
                attributes, baseType);

            mainModule.Types.Add(resultType);
            return resultType;
        }

        public static MethodReference ImportConstructor<T>(this ModuleDefinition module,
            params Type[] constructorParameters)
        {
            return module.Import(typeof (T).GetConstructor(constructorParameters));
        }

        public static MethodReference ImportMethod(this ModuleDefinition module, string methodName, Type declaringType)
        {
            return module.Import(declaringType.GetMethod(methodName));
        }

        
        public static MethodReference ImportMethod(this ModuleDefinition module, string methodName, Type declaringType,
            BindingFlags flags)
        {
            return module.Import(declaringType.GetMethod(methodName, flags));
        }
        
        public static MethodReference ImportMethod<T>(this ModuleDefinition module, string methodName)
        {
            return module.Import(typeof (T).GetMethod(methodName));
        }

        public static MethodReference ImportMethod(this ModuleDefinition module, string methodName, 
            Type declaringType, params Type[] parameterTypes)
        {
            return module.Import(declaringType.GetMethod(methodName, parameterTypes));
        }

        public static MethodReference ImportMethod<T>(this ModuleDefinition module, string methodName,
            params Type[] parameterTypes)
        {
            return module.Import(typeof (T).GetMethod(methodName, parameterTypes));
        }

        public static MethodReference ImportMethod<T>(this ModuleDefinition module,
            string methodName, BindingFlags flags)
        {
            return module.Import(typeof (T).GetMethod(methodName, flags));
        }

        public static TypeReference ImportType<T>(this ModuleDefinition module)
        {
            return module.Import(typeof (T));
        }

        public static TypeReference ImportType(this ModuleDefinition module, Type targetType)
        {
            return module.Import(targetType);
        }

        public static TypeDefinition GetType(this ModuleDefinition module, string typeName)
        {
            var result = (from TypeDefinition t in module.Types
                where t.Name == typeName
                select t).FirstOrDefault();

            return result;
        }
    }
}