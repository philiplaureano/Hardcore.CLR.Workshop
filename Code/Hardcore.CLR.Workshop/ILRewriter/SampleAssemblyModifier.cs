using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ILRewriter
{
    public class SampleAssemblyModifier : IAssemblyModifier
    {
        public void Modify(AssemblyDefinition assembly)
        {
            var mainModule = assembly.MainModule;

            var targetType = mainModule.Types.FirstOrDefault(t => t.Name == "SampleClassWithInstanceMethod");
            if (targetType == null)
                return;

            var targetMethod = targetType.Methods.First(m => m.Name == "DoSomething");
            var methodBody = targetMethod.Body;            

            var instructions = methodBody.Instructions;
            foreach (var instruction in instructions)
            {
                if (instruction.OpCode != OpCodes.Ldstr)
                    continue;

                // Replace the Hello World string
                var operand = instruction.Operand as string;
                if (operand != null && operand == "Hello, World!")
                {
                    instruction.Operand = "Hello, NDC Hardcore CLR Workshop!";
                }
            }
        }
    }
}