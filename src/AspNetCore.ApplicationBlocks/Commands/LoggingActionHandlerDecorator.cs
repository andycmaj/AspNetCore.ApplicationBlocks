using SerilogEventLogger;

namespace AspNetCore.ApplicationBlocks.Commands
{
    internal class LoggingActionHandlerDecorator<TCommand> :
        LoggingCommandHandlerDecorator<TCommand>, IActionHandler<TCommand>
        where TCommand : IAction
    {
        private readonly IActionHandler<TCommand> decorateeHandler;

        public LoggingActionHandlerDecorator(
            IActionHandler<TCommand> decorateeHandler,
            IEventLogger<LoggingActionHandlerDecorator<TCommand>> logger
        ) : base(decorateeHandler, logger)
        {
            this.decorateeHandler = decorateeHandler;
        }

        public void Execute(TCommand action)
        {
            using (WithLogging())
            {
                decorateeHandler.Execute(action);
            }
        }
    }
}