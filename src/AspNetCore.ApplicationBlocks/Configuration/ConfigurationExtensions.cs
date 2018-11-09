using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SimpleInjector;

namespace AspNetCore.ApplicationBlocks.Configuration
{
    /// <summary>
    /// Configuration Application Block
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Adds Configuration to your application.
        /// </summary>
        /// <param name="container">
        /// The <see cref="Container" /> serving as the composition
        /// root for your application.
        /// </param>
        /// <param name="frameworkConfiguration"></param>
        /// <param name="hostingEnvironment"></param>
        /// <typeparam name="TAppConfigInterface">
        /// Your custom app config interface.
        /// Must implement <see cref="IApplicationConfiguration" />
        /// </typeparam>
        /// <typeparam name="TAppConfigImplementation">
        /// Your custom app config implementation
        /// Must implement <typeparamref name="TAppConfigInterface" />.
        /// </typeparam>
        /// <returns>
        /// The <paramref name="container">
        /// Container</paramref> being used as the composition root
        /// of your application.
        /// </returns>
        public static Container AddApplicationConfiguration<TAppConfigInterface, TAppConfigImplementation>(
            this Container container,
            IConfiguration frameworkConfiguration,
            IHostingEnvironment hostingEnvironment
        )
            where TAppConfigInterface : class, IApplicationConfiguration
            where TAppConfigImplementation : class, TAppConfigInterface
        {
            new ConfigurationModule<TAppConfigInterface, TAppConfigImplementation>(
                frameworkConfiguration,
                hostingEnvironment
            ).RegisterServices(container);

            return container;
        }
    }
}