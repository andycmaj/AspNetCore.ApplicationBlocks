using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using SerilogEventLogger;
using SimpleInjector;

namespace AspNetCore.ApplicationBlocks.Logging
{
    /// <summary>
    /// Logging Application Block
    /// </summary>
    public static class LoggingExtensions
    {
        /// <summary>
        /// Adds Event Logging to your application.
        /// </summary>
        /// <param name="container">The composition root of your application</param>
        /// <param name="configureSerilogConfig">
        /// Action used to apply additional configuration to the serilog logger config
        /// </param>
        /// <returns>
        /// The <paramref name="container">
        /// Container</paramref> being used as the composition root
        /// of your application.
        /// </returns>
        public static Container AddEventLogging(
            this Container container,
            Action<LoggerConfiguration> configureSerilogConfig = null
        )
        {
            return AddEventLogging(container, Lifestyle.Scoped, configureSerilogConfig);
        }

        /// <summary>
        /// Adds Event Logging to your application.
        /// </summary>
        /// <param name="container">The composition root of your application</param>
        /// <param name="configureSerilogConfig">
        /// Action used to apply additional configuration to the serilog logger config
        /// </param>
        /// <param name="defaultLifestyle">Default Lifestyle with which to create loggers</param>
        /// <returns>
        /// The <paramref name="container">
        /// Container</paramref> being used as the composition root
        /// of your application.
        /// </returns>
        public static Container AddEventLogging(
            this Container container,
            Lifestyle defaultLifestyle,
            Action<LoggerConfiguration> configureSerilogConfig = null
        )
        {
            new LoggingModule(
                defaultLifestyle,
                configureSerilogConfig
            ).RegisterServices(container);

            return container;
        }
    }
}