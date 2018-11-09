
using AspNetCore.ApplicationBlocks.Commands;

namespace AspNetCore.ApplicationBlocks.TestSupport.TestTransitiveDependencyContext
{
    public class Action2 : IAction
    {
        public class Handler : IActionHandler<Action2>
        {
            public void Execute(Action2 action)
            {
            }
        }
    }
}