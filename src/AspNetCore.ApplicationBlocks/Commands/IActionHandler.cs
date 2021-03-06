namespace AspNetCore.ApplicationBlocks.Commands
{
    /// <summary>
    /// Handles execution of IActions of type TAction.
    /// </summary>
    /// <typeparam name="TAction">
    /// The type of IAction this IActionHandler can execute.
    /// </typeparam>
    public interface IActionHandler<in TAction> : ICommandHandler
        where TAction : IAction
    {
        /// <summary>
        /// Execute the specified IAction instance.
        /// </summary>
        void Execute(TAction action);
    }
}
