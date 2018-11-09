using SimpleInjector;
using Microsoft.Extensions.DependencyModel;
using AspNetCore.ApplicationBlocks.DependencyInjection;

namespace AspNetCore.ApplicationBlocks.Bootstrapping
{
    /// <summary>
    /// Discovers and registers all IBootstraper implementations in all TheChunnel.*
    /// assemblies.
    /// </summary>
    public class BootstrapperModule : IModule
    {
        private DependencyContext dependencyContext;

        /// <summary>
        /// Create an instance of <c>BootstrapperModule</c> which will
        /// discover and register all <see cref="IBootstrapper">IBootstrappers</see>
        /// in the given <see cref="DependencyContext" />.
        /// </summary>
        /// <param name="dependencyContext">
        /// Optional: An alternate <see cref="DependencyContext" /> to search when
        /// attempting to discover <see cref="IBootstrapper">IBootstrappers</see>
        /// to register.
        /// (Default: <see cref="DependencyContext.Default" />)
        /// </param>
        public BootstrapperModule(
            DependencyContext dependencyContext = null
        )
        {
            this.dependencyContext = dependencyContext ?? DependencyContext.Default;
        }

        /// <summary>
        /// Discover and register <see cref="IBootstrapper">IBootstrappers</see>.
        /// </summary>
        /// <param name="container">DI container</param>
        public void RegisterServices(Container container)
        {
            var assemblies = dependencyContext
                .CompileLibraries
                .GetLoadableAssemblies();

            container.Collection.Register<IBootstrapper>(assemblies);
        }
    }
}