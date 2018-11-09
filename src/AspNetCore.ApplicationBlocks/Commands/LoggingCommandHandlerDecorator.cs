using SerilogEventLogger;

namespace AspNetCore.ApplicationBlocks.Commands
{
    internal abstract class LoggingCommandHandlerDecorator<TCommand>
    {
        private readonly object decoratee;
        private readonly bool isAsync;

        public IEventLogger Logger { get; }

        protected LoggingCommandHandlerDecorator(object decoratee, IEventLogger logger, bool isAsync = false)
        {
            this.decoratee = decoratee;
            this.isAsync = isAsync;
            Logger = logger;
        }

        protected IScopedMetric WithLogging()
        {
            var data = new
            {
                CommandType = typeof(TCommand).FullName,
                HandlerType = decoratee.GetType().FullName,
                IsAsync = isAsync
            };

            return Logger.StartTimer("ExecuteCommand", data);
        }
    }
}