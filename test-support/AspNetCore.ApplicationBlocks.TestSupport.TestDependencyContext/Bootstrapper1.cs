using AspNetCore.ApplicationBlocks.Bootstrapping;

namespace AspNetCore.ApplicationBlocks.TestSupport.TestDependencyContext
{
    public class Bootstrapper1 : IBootstrapper
    {
        private readonly ITestService service;

        public Bootstrapper1(ITestService service)
        {
            this.service = service;
        }

        public void Bootstrap()
        {
            service.DoSomething();
        }
    }
}