using SerilogEventLogger;

namespace AspNetCore.ApplicationBlocks.Commands
{
    internal class LoggingFunctionDecorator<TCommand, TOutput> :
        LoggingCommandHandlerDecorator<TCommand>, IFunctionHandler<TCommand, TOutput>
        where TCommand : IFunction<TOutput>
    {
        private readonly IFunctionHandler<TCommand, TOutput> decorateeHandler;

        public LoggingFunctionDecorator(
            IFunctionHandler<TCommand, TOutput> decorateeHandler,
            IEventLogger<LoggingFunctionDecorator<TCommand, TOutput>> logger
        ) : base(decorateeHandler, logger, true)
        {
            this.decorateeHandler = decorateeHandler;
        }

        public TOutput Execute(TCommand action)
        {
            using (WithLogging())
            {
                return decorateeHandler.Execute(action);
            }
        }
    }
}