using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ILRewriter
{
    public static class AssemblyDefinitionExtensions
    {
        public static Assembly ToAssembly(this AssemblyDefinition definition)
        {
            Assembly result = null;
            using (var stream = new MemoryStream())
            {
                // Persist the assembly to the stream
                definition.Write(stream);
                var buffer = stream.GetBuffer();
                result = Assembly.Load(buffer);
            }

            return result;
        }
    }
}
