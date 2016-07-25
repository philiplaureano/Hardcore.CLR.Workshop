using Mono.Cecil;

namespace ILRewriter.Extensions
{
    public static class ParameterDefinitionExtensions
    {
        public static bool IsByRef(this ParameterDefinition parameter)
        {
            return parameter.ParameterType != null && parameter.ParameterType.IsByReference;
        }
    }
}