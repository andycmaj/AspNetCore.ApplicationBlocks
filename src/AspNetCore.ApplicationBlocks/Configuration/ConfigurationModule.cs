using AspNetCore.ApplicationBlocks.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using SimpleInjector;

namespace AspNetCore.ApplicationBlocks.Configuration
{
    internal class ConfigurationModule<TAppConfigInterface, TAppConfigImplementation> : IModule
        where TAppConfigInterface : class, IApplicationConfiguration
        where TAppConfigImplementation : class, TAppConfigInterface
    {
        // TODO: move to AwsModule
        // private readonly AWSCredentials awsCreds = FallbackCredentialsFactory.GetCredentials(true);

        private readonly IConfiguration frameworkConfiguration;
        private readonly IHostingEnvironment hostingEnvironment;

        public ConfigurationModule(
            IConfiguration frameworkConfiguration,
            IHostingEnvironment hostingEnvironment
        )
        {
            this.frameworkConfiguration = frameworkConfiguration;
            this.hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Create Config providers
        /// https://docs.asp.net/en/latest/fundamentals/configuration.html
        /// Order of config sources matters. Last source wins.
        /// </summary>
        /// <param name="container">DI container</param>
        public void RegisterServices(Container container)
        {
            container.RegisterInstance(frameworkConfiguration);
            container.RegisterInstance(hostingEnvironment);

            container.RegisterSingleton<IApplicationConfiguration, TAppConfigImplementation>();

            if (typeof(TAppConfigInterface) != typeof(IApplicationConfiguration))
            {
                container.RegisterSingleton<TAppConfigInterface, TAppConfigImplementation>();
            }
        }
    }
}
