using AspNetCore.ApplicationBlocks.Commands;

namespace AspNetCore.ApplicationBlocks.TestSupport.TestDependencyContext
{
    public class Action1 : IAction
    {
        public class Handler : IActionHandler<Action1>
        {
            private readonly ITestService service;

            public Handler(ITestService service)
            {
                this.service = service;
            }

            public void Execute(Action1 action)
            {
                service.DoSomething();
            }
        }
    }
}