using System;
using AspNetCore.ApplicationBlocks.Configuration;
using AspNetCore.ApplicationBlocks.DependencyInjection;
using SerilogEventLogger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;
using SimpleInjector;

namespace AspNetCore.ApplicationBlocks.Logging
{
    internal class LoggingModule : IModule
    {
        private readonly IConfiguration frameworkConfiguration;
        private readonly Lifestyle defaultLifestyle;
        private readonly bool logToConsole;
        private readonly Type[] customExclusions;

        public LoggingModule(
            IConfiguration frameworkConfiguration,
            Lifestyle defaultLifestyle = null,
            bool logToConsole = false,
            params Type[] customExclusions
        )
        {
            this.frameworkConfiguration = frameworkConfiguration;
            this.defaultLifestyle = defaultLifestyle ?? Lifestyle.Scoped;
            this.logToConsole = logToConsole;
            this.customExclusions = customExclusions;
        }

        public void RegisterServices(Container container)
        {
            container.Register(typeof(IEventLogger<>), typeof(EventLogger<>), defaultLifestyle);

            RegisterWithConfig(
                container,
                config =>
                    new LoggingConfiguration(
                        config.Environment,
                        config.Application,
                        config.Version,
                        config.Hostname
                    ),
                Lifestyle.Singleton
            );

            container.RegisterSingleton(() =>
            {
                var serilogConfig =
                    container.GetInstance<LoggingConfiguration>().SerilogConfiguration;

                foreach (var customExclusion in customExclusions)
                {
                    serilogConfig.Filter.ByExcluding(Matching.FromSource(customExclusion.FullName));
                }

                Log.Logger = serilogConfig.CreateLogger();

                return Log.Logger;
            });
        }

        private static void RegisterWithConfig<TService>(
            Container container,
            Func<IApplicationConfiguration, TService> factory,
            Lifestyle lifestyle = null
        )
            where TService : class
        {
            container.Register(
                () => factory(container.GetInstance<IApplicationConfiguration>()),
                lifestyle ?? Lifestyle.Transient
            );
        }
    }
}
