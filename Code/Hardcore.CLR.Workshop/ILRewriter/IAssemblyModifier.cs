using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Mono.Cecil;

namespace ILRewriter
{
    public interface IAssemblyModifier
    {
        void Modify(AssemblyDefinition assembly);
    }    
}