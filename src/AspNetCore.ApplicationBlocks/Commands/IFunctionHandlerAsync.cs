using System.Threading.Tasks;

namespace AspNetCore.ApplicationBlocks.Commands
{
    public interface IFunctionHandlerAsync<in TFunction, TOutput> : ICommandHandler
        where TFunction : IFunction<TOutput>
    {

        /// <summary>
        /// Execute the specified IFunction instance and return a result
        /// of type TOutput.
        /// </summary>
        Task<TOutput> ExecuteAsync(TFunction function);
    }
}
