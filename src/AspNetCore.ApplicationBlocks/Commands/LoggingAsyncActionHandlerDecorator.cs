using System.Threading.Tasks;
using SerilogEventLogger;

namespace AspNetCore.ApplicationBlocks.Commands
{
    internal class LoggingAsyncActionHandlerDecorator<TCommand> :
        LoggingCommandHandlerDecorator<TCommand>, IActionHandlerAsync<TCommand>
        where TCommand : IAction
    {
        private readonly IActionHandlerAsync<TCommand> decorateeHandler;

        public LoggingAsyncActionHandlerDecorator(
            IActionHandlerAsync<TCommand> decorateeHandler,
            IEventLogger<LoggingAsyncActionHandlerDecorator<TCommand>> logger
        ) : base(decorateeHandler, logger, true)
        {
            this.decorateeHandler = decorateeHandler;
        }

        public async Task ExecuteAsync(TCommand action)
        {
            using (WithLogging())
            {
                await decorateeHandler.ExecuteAsync(action);
            }
        }
    }
}