using AspNetCore.ApplicationBlocks.Bootstrapping;

namespace AspNetCore.ApplicationBlocks.TestSupport.TestDependencyContext
{
    public class Bootstrapper2 : IBootstrapper
    {
        private readonly ITestService service;

        public Bootstrapper2(ITestService service)
        {
            this.service = service;
        }

        public void Bootstrap()
        {
            service.DoSomething();
        }
    }
}