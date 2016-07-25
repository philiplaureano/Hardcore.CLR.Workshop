using System;
using System.Linq;
using ILRewriter;
using Mono.Cecil;
using NUnit.Framework;

namespace Tests
{
    public abstract class BaseCecilTestFixture : BaseAssemblyVerificationTestFixture
    {
        private AssemblyDefinition RewriteAssemblyOf<T>()
        {
            var assemblyLocation = typeof(T).Assembly.Location;
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyLocation);

            var modifier = GetAssemblyModifier();
            modifier.Modify(assemblyDefinition);

            return assemblyDefinition;
        }

        private static dynamic CreateModifiedType(AssemblyDefinition assemblyDefinition, string typeName)
        {
            var assembly = assemblyDefinition.ToAssembly();
            var targetType = assembly.GetTypes().First(t => t.Name == typeName);
            dynamic instance = Activator.CreateInstance(targetType);
            return instance;
        }

        protected abstract IAssemblyModifier GetAssemblyModifier();
    }
}