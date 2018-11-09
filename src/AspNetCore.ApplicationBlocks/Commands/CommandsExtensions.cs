using Microsoft.Extensions.DependencyModel;
using SimpleInjector;

namespace AspNetCore.ApplicationBlocks.Commands
{
    /// <summary>
    /// Commands Application Block
    /// </summary>
    public static class CommandsExtensions
    {
        /// <summary>
        /// Discovers and registers all <a href="">Commands and Command Handlers</a>
        /// found in a given <see cref="DependencyContext" />, as well as supporting
        /// types needed to inject and use <see cref="ICommandHandler" />.
        /// </summary>
        /// <param name="container">
        /// The <see cref="Container" /> serving as the composition
        /// root for your application.
        /// </param>
        /// <param name="commandLifestyle">
        /// Optional: The default <see cref="Lifestyle" /> to use when registering Commands.
        /// (Default: <see cref="Lifestyle.Transient" />)
        /// </param>
        /// <param name="discoveryContext">
        /// Optional: The <see cref="DependencyContext" /> to search when attempting to
        /// discover Commands to register.
        /// (Default: <see cref="DependencyContext.Default" />)
        /// </param>
        /// <returns>
        /// The <paramref name="container">
        /// Container</paramref> being used as the composition root
        /// of your application.
        /// </returns>
        public static Container AddCommands(
            this Container container,
            Lifestyle commandLifestyle = null,
            DependencyContext discoveryContext = null
        )
        {
            new CommandAutoRegistryModule(
                commandLifestyle ?? Lifestyle.Transient,
                discoveryContext ?? DependencyContext.Default
            ).RegisterServices(container);

            return container;
        }
    }
}