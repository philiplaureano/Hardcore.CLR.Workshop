using System.Reflection;

namespace ILRewriter
{
    public interface IInterceptor
    {
        object Intercept(object targetInstance, object[] args, MethodBase targetMethod);
    }
}