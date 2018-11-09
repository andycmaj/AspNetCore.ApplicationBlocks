using System.Threading.Tasks;
using SerilogEventLogger;

namespace AspNetCore.ApplicationBlocks.Commands
{
    internal class LoggingAsyncFunctionDecorator<TCommand, TOutput> :
        LoggingCommandHandlerDecorator<TCommand>, IFunctionHandlerAsync<TCommand, TOutput>
        where TCommand : IFunction<TOutput>
    {
        private readonly IFunctionHandlerAsync<TCommand, TOutput> decorateeHandler;

        public LoggingAsyncFunctionDecorator(
            IFunctionHandlerAsync<TCommand, TOutput> decorateeHandler,
            IEventLogger<LoggingAsyncFunctionDecorator<TCommand, TOutput>> logger
        ) : base(decorateeHandler, logger, true)
        {
            this.decorateeHandler = decorateeHandler;
        }

        public async Task<TOutput> ExecuteAsync(TCommand action)
        {
            using (WithLogging())
            {
                return await decorateeHandler.ExecuteAsync(action);
            }
        }
    }
}