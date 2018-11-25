using SimpleInjector;
using System.Collections.Generic;
using SimpleInjector.Lifestyles;
using AspNetCore.ApplicationBlocks.Bootstrapping;
using AspNetCore.ApplicationBlocks.Configuration;
using AspNetCore.ApplicationBlocks.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using System;
using AspNetCore.ApplicationBlocks.Logging;
using AspNetCore.ApplicationBlocks.Commands;
using System.IO;

namespace AspNetCore.ApplicationBlocks.Console
{
    /// <summary>
    /// Used to initialize a <see cref="ConsoleApplicationContext" />.
    /// </summary>
    /// <remarks>
    /// Serves as the analog to <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/hosting#setting-up-a-host">
    /// AspNetCore's <c>WebHost.CreateDefaultBuilder</c></a>, and <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/startup">
    /// <c>Startup</c></a>, but for console applications.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Do your dependency registration in IModules.
    /// public class ServiceModule : IModule
    /// {
    ///     public void RegisterServices(Container container)
    ///     {
    ///         container.Register&lt;Service&gt;();
    ///     }
    /// }
    /// public class Program
    /// {
    ///     public static async Task Main(string[] args)
    ///     {
    ///         using (var context = new ConsoleApplicationBuilder(
    ///             // Add your IModules here to register their dependencies into
    ///             // the ConsoleApplicationContext's Container instance.
    ///             new ServiceModule()
    ///         ).Build&lt;IAppConfig, AppConfig&gt;())
    ///         {
    ///             // Access the Container via ConsoleApplicationContext to get
    ///             // an instance of your console application's entry point.
    ///             await context.Container.GetInstance&lt;Service&gt;().RunAsync();
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ConsoleApplicationBuilder
    {
        private const string DefaultConfigFileName = "appsettings";

        private readonly Lifestyle defaultLifestyle;
        private readonly IModule[] modules;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly IConfigurationBuilder frameworkConfigurationBuilder;
        /// <summary>
        /// Create a <c>ConsoleApplicationBuilder</c> given an optional
        /// collection of <see cref="IModule">IModules</see>.
        /// </summary>
        /// <param name="modules">
        /// Optional: The <see cref="IModule">IModules</see>
        /// used to compose the composition root
        /// </param>
        public ConsoleApplicationBuilder(params IModule[] modules)
            : this(Lifestyle.Scoped, modules)
        {
        }

        /// <summary>
        /// Create a <c>ConsoleApplicationBuilder</c> given an optional
        /// collection of <see cref="IModule">IModules</see>.
        /// </summary>
        /// <param name="configFileName">Name, without extension, of your json config file.</param>
        /// <param name="modules">
        /// Optional: The <see cref="IModule">IModules</see>
        /// used to compose the composition root
        /// </param>
        public ConsoleApplicationBuilder(string configFileName, params IModule[] modules)
            : this(Directory.GetCurrentDirectory(), Lifestyle.Scoped, configFileName, modules)
        {
        }

        /// <summary>
        /// Create a <c>ConsoleApplicationBuilder</c> given a default
        /// <see cref="Lifestyle" /> and an optional collection of
        /// <see cref="IModule">IModules</see>
        /// </summary>
        /// <param name="defaultLifestyle">
        /// The default <see cref="Lifestyle" /> to use when registering
        /// services in the <see cref="Container" />
        /// </param>
        /// <param name="modules">
        /// Optional: The <see cref="IModule">IModules</see>
        /// used to compose the composition root
        /// </param>
        public ConsoleApplicationBuilder(Lifestyle defaultLifestyle, params IModule[] modules)
            : this(Directory.GetCurrentDirectory(), defaultLifestyle, DefaultConfigFileName, modules)
        {
        }

        /// <summary>
        /// Create a <c>ConsoleApplicationBuilder</c> given a default
        /// <see cref="Lifestyle" /> and an optional collection of
        /// <see cref="IModule">IModules</see>
        /// </summary>
        /// <param name="configBasePath">
        /// Directory to use as config root when initializing
        /// the framework <see cref="ConfigurationBuilder" />
        /// </param>
        /// <param name="defaultLifestyle">
        /// The default <see cref="Lifestyle" /> to use when registering
        /// services in the <see cref="Container" />
        /// </param>
        /// <param name="configFileName">Name, without extension, of your json config file.</param>
        /// <param name="modules">
        /// Optional: The <see cref="IModule">IModules</see>
        /// used to compose the composition root
        /// </param>
        public ConsoleApplicationBuilder(
            string configBasePath,
            Lifestyle defaultLifestyle,
            string configFileName,
            params IModule[] modules
        )
        {
            this.defaultLifestyle = defaultLifestyle;
            this.modules = modules;

            // TODO: Emulate WebHost.CreateDefaultBuilder() until ASPNETCORE 2.1,
            // when this can be replaced with generic Hosting.
            // see https://www.stevejgordon.co.uk/using-generic-host-in-dotnet-core-console-based-microservices
            hostingEnvironment = CreateHostingEnvironment();

            frameworkConfigurationBuilder =
                new ConfigurationBuilder()
                    .SetBasePath(configBasePath)
                    .AddJsonFile($"{configFileName}.json", optional: true, reloadOnChange: false)
                    .AddJsonFile($"{configFileName}.{hostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();
        }

        /// <summary>
        /// Perform additional configuration on the AspNetCore framework
        /// <see cref="IConfigurationBuilder" />.
        /// </summary>
        /// <param name="configureConfigurationBuilder">
        /// An <c>Action</c> of type
        /// <see cref="IConfigurationBuilder" /> that's passed the internal framework
        /// configuration builder for you to configure.
        /// </param>
        /// <returns>This <c>ConsoleApplicationBuilder</c></returns>
        public ConsoleApplicationBuilder ConfigureConfigurationBuilder(Action<IConfigurationBuilder> configureConfigurationBuilder)
        {
            configureConfigurationBuilder(frameworkConfigurationBuilder);

            return this;
        }

        private static IHostingEnvironment CreateHostingEnvironment()
        {
            var hostingEnvironment = new HostingEnvironment
            {
                EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            };

            return hostingEnvironment;
        }

        /// <summary>
        /// Configure the Application Blocks and <see cref="Container">composition root</see>
        /// and build the application's <see cref="ConsoleApplicationContext" />.
        /// </summary>
        /// <param name="dependencyContext">
        /// Optional: A <see cref="DependencyContext" /> to use for all registration
        /// discovery.
        /// </param>
        /// <returns>
        /// The <see cref="ConsoleApplicationContext" /> that will serve as the run context of
        /// your console application
        /// </returns>
        public ConsoleApplicationContext Build<TAppConfigInterface, TAppConfigImplementation>(
            DependencyContext dependencyContext = null
        )
            where TAppConfigInterface : class, IApplicationConfiguration
            where TAppConfigImplementation : class, TAppConfigInterface
        {
            var container = new Container();

            container.Options.DefaultLifestyle = defaultLifestyle;
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            var frameworkConfiguration = frameworkConfigurationBuilder.Build();

            container
                .AddApplicationConfiguration<TAppConfigInterface, TAppConfigImplementation>(frameworkConfiguration, hostingEnvironment)
                .AddEventLogging(defaultLifestyle)
                .AddCommands(defaultLifestyle, dependencyContext)
                .AddBootstrappers(dependencyContext);

            foreach (var module in modules)
            {
                module.RegisterServices(container);
            }

            container.Verify();

            if (container.GetRegistration(typeof(IEnumerable<IBootstrapper>)) != null)
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    // Resolve and run IBootstrappers
                    foreach (var bootstrapper in container.GetAllInstances<IBootstrapper>())
                    {
                        bootstrapper.Bootstrap();
                    }
                }
            }

            return new ConsoleApplicationContext(container);
        }
    }
}
