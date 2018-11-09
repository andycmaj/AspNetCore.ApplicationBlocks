using AspNetCore.ApplicationBlocks.Commands;

namespace AspNetCore.ApplicationBlocks.TestSupport.TestDependencyContext
{
    public class Function1 : IFunction<bool>
    {
        public class Handler : IFunctionHandler<Function1, bool>
        {
            private readonly ITestService service;

            public Handler(ITestService service)
            {
                this.service = service;
            }

            public bool Execute(Function1 action)
            {
                service.DoSomething();

                return true;
            }
        }
    }
}