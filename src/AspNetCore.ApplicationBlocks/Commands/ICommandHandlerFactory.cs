using System;

namespace AspNetCore.ApplicationBlocks.Commands
{
    public interface ICommandHandlerFactory
    {
        ICommandHandler Create(Type handlerType);

        void Release(ICommandHandler handler);
    }
}
