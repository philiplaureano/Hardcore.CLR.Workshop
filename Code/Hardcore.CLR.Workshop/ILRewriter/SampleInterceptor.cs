using System.Reflection;

namespace ILRewriter
{
    public class SampleInterceptor : IInterceptor
    {
        public object Intercept(object targetInstance, object[] args,
            MethodBase targetMethod)
        {
            return 42;
        }
    }
}