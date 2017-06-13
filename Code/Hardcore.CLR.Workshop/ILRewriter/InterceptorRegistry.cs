namespace ILRewriter
{
    public static class InterceptorRegistry
    {
        public static IInterceptor GetInterceptor() => new SampleInterceptor();
    }
}