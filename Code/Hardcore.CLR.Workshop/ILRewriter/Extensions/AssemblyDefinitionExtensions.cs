using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace ILRewriter.Extensions
{
    public static class AssemblyDefinitionExtensions
    {
        public static Assembly ToAssembly(this AssemblyDefinition definition)
        {
            Assembly result = null;
            using (var stream = new MemoryStream())
            {
                // Persist the assembly to the stream and load it into memory
                definition.Write(stream);
                var buffer = stream.GetBuffer();
                result = Assembly.Load(buffer);
            }

            return result;
        }
    }
}
