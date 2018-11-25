using System;
using System.Collections.Generic;
using SerilogEventLogger;
using Microsoft.Extensions.DependencyModel;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using AspNetCore.ApplicationBlocks.Bootstrapping;

namespace AspNetCore.ApplicationBlocks
{
    /// <summary>
    /// Bootstrapping Application Block
    /// </summary>
    public static class BootstrappingExtensions
    {
        /// <summary>
        /// Adds Bootstrap actions to your application by discovering and registering all
        /// <see cref="IBootstrapper">IBoostrappers</see>
        /// found in the given <see cref="DependencyContext">DependencyContext</see>.
        /// </summary>
        /// <param name="container">The composition root of your application</param>
        /// <param name="discoveryContext">
        /// Optional: The <see cref="DependencyContext">DependencyContext</see> to search for
        /// <see cref="IBootstrapper">IBoostrappers</see>.
        /// If not specified, will use <c>DependencyContext.Default</c>.
        /// </param>
        /// <returns>
        /// The <paramref name="container">Container</paramref> being used as the composition root
        /// of your application.
        /// </returns>
        public static Container AddBootstrappers(
            this Container container,
            DependencyContext discoveryContext = null
        )
        {
            new BootstrapperModule(
                discoveryContext ?? DependencyContext.Default
            ).RegisterServices(container);

            return container;
        }

        /// <summary>
        /// Resolve and execute all <see cref="IBootstrapper">IBootstrappers</see>
        /// registered by <see cref="AddBootstrappers(Container, DependencyContext)">
        /// container.AddBootstrappers(...)</see>
        /// </summary>
        /// <param name="container">The <c>Container</c></param>
        public static void RunBootstrappers(this Container container)
        {
            using (AsyncScopedLifestyle.BeginScope(container))
            {
                var logger = container.GetInstance<IEventLogger<IBootstrapper>>();

                IEnumerable<IBootstrapper> bootstrappers;
                try
                {
                    bootstrappers = container.GetAllInstances<IBootstrapper>();
                }
                catch (ActivationException ex) when (ex.Message.StartsWith("No registration", StringComparison.OrdinalIgnoreCase))
                {
                    logger.DebugEvent("NoBootstrappersFound");
                    return;
                }

                // Resolve and run IBootstrappers
                foreach (var bootstrapper in bootstrappers)
                {
                    using (logger.BeginScope(new { Bootstrapper = bootstrapper.GetType().FullName }))
                    {
                        bootstrapper.Bootstrap();
                    }
                }
            }
        }
    }
}