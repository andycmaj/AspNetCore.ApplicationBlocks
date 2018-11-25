using System;
using AspNetCore.ApplicationBlocks.Configuration;
using AspNetCore.ApplicationBlocks.DependencyInjection;
using SerilogEventLogger;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Filters;
using SimpleInjector;

namespace AspNetCore.ApplicationBlocks.Logging
{
    internal class LoggingModule : IModule
    {
        private readonly Lifestyle defaultLifestyle;
        private readonly Action<LoggerConfiguration> configureSerilogLogger;

        public LoggingModule(
            Lifestyle defaultLifestyle = null,
            Action<LoggerConfiguration> configureSerilogLogger = null
        )
        {
            this.defaultLifestyle = defaultLifestyle ?? Lifestyle.Scoped;
            this.configureSerilogLogger = configureSerilogLogger;
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

                if (configureSerilogLogger != null)
                {
                    configureSerilogLogger(serilogConfig);
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
