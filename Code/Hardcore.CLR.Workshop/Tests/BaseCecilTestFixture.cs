using System;
using System.Linq;
using ILRewriter;
using ILRewriter.Extensions;
using Mono.Cecil;

namespace Tests
{
    public abstract class BaseCecilTestFixture : BaseAssemblyVerificationTestFixture
    {
        protected AssemblyDefinition RewriteAssemblyOf<T>()
        {
            var assemblyLocation = typeof(T).Assembly.Location;
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyLocation);

            var modifier = GetAssemblyModifier();
            modifier.Modify(assemblyDefinition);

            return assemblyDefinition;
        }

        protected static dynamic CreateModifiedType(AssemblyDefinition assemblyDefinition, string typeName)
        {
            var assembly = assemblyDefinition.ToAssembly();
            var targetType = assembly.GetTypes().First(t => t.Name == typeName);
            dynamic instance = Activator.CreateInstance(targetType);
            return instance;
        }

        protected abstract IAssemblyModifier GetAssemblyModifier();
    }
}