
using AspNetCore.ApplicationBlocks.Commands;

namespace AspNetCore.ApplicationBlocks.TestSupport.TestTransitiveDependencyContext
{
    public class Function2 : IFunction<bool>
    {
        public class Handler : IFunctionHandler<Function2, bool>
        {
            public bool Execute(Function2 action)
            {
                return true;
            }
        }
    }
}