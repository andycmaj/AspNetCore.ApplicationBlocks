using System;
using Microsoft.Extensions.Configuration;
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
        /// <remarks>
        /// Requires that your application has been bootstrapped with default ASPNETCORE framework
        /// <see cref="Microsoft.Extensions.Configuration.IConfiguration">IConfiguration</see> and
        /// <see cref="Microsoft.Extensions.Logging.ILoggerFactory">ILoggerFactory</see>. These will
        /// function as our interfaces with low-level framework logging and configuration. See
        /// <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/hosting#setting-up-a-host">
        /// Setting up a host</a> for more info on bootstrapping your application.
        /// </remarks>
        /// <example>
        /// <code>
        /// // appsettings.json sections for logging to splunk via kinesis
        /// "Logging": {
        ///   // log to splunk via kinesis
        ///   "Kinesis": {
        ///     "AccessKey": "",
        ///     "SecretKey": "",
        ///     "Region": "us-west-2",
        ///     "StreamName": "development_splunk"
        ///   },
        ///   // log directly to splunk, ie for local eventlog testing
        ///   "Splunk": {
        ///     "EventCollectorUrl": "",
        ///     "Token": "EF211A51-D6AC-4045-8CD6-F730939AC518"
        ///   },
        /// }
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// # Example of setting Splunk logging config via environment vars
        /// export LOGGING__KINESIS__ACCESSKEY=....
        /// export LOGGING__KINESIS__SECRETKEY=....
        /// export LOGGING__KINESIS__STREAMNAME=production_splunk
        /// export LOGGING__KINESIS__REGION=us-west-2
        /// </code>
        /// </example>
        /// <param name="container">The composition root of your application</param>
        /// <param name="frameworkConfiguration">
        /// The <see cref="Microsoft.Extensions.Configuration.IConfiguration">IConfiguration</see>
        /// used to provide logging configuration values from various configuration sources.
        /// </param>
        /// <param name="shouldLogToConsole">Optional: Determines whether to pipe all events through the console. <c>true</c> by default.</param>
        /// <param name="excludedLoggerTypes">Optional: Types to exclude as log sources</param>
        /// <returns>
        /// The <paramref name="container">
        /// Container</paramref> being used as the composition root
        /// of your application.
        /// </returns>
        public static Container AddEventLogging(
            this Container container,
            IConfiguration frameworkConfiguration,
            bool shouldLogToConsole = true,
            params Type[] excludedLoggerTypes
        )
        {
            return AddEventLogging(container, frameworkConfiguration, Lifestyle.Scoped, shouldLogToConsole, excludedLoggerTypes);
        }

        /// <summary>
        /// Adds Event Logging to your application.
        /// </summary>
        /// <remarks>
        /// Requires that your application has been bootstrapped with default ASPNETCORE framework
        /// <see cref="Microsoft.Extensions.Configuration.IConfiguration">IConfiguration</see> and
        /// <see cref="Microsoft.Extensions.Logging.ILoggerFactory">ILoggerFactory</see>. These will
        /// function as our interfaces with low-level framework logging and configuration. See
        /// <a href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/hosting#setting-up-a-host">
        /// Setting up a host</a> for more info on bootstrapping your application.
        /// </remarks>
        /// <example>
        /// <code>
        /// // appsettings.json sections for logging to splunk via kinesis
        /// "Logging": {
        ///   // log to splunk via kinesis
        ///   "Kinesis": {
        ///     "AccessKey": "",
        ///     "SecretKey": "",
        ///     "Region": "us-west-2",
        ///     "StreamName": "development_splunk"
        ///   },
        ///   // log directly to splunk, ie for local eventlog testing
        ///   "Splunk": {
        ///     "EventCollectorUrl": "",
        ///     "Token": "EF211A51-D6AC-4045-8CD6-F730939AC518"
        ///   },
        /// }
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// # Example of setting Splunk logging config via environment vars
        /// export LOGGING__KINESIS__ACCESSKEY=....
        /// export LOGGING__KINESIS__SECRETKEY=....
        /// export LOGGING__KINESIS__STREAMNAME=production_splunk
        /// export LOGGING__KINESIS__REGION=us-west-2
        /// </code>
        /// </example>
        /// <param name="container">The composition root of your application</param>
        /// <param name="frameworkConfiguration">
        /// The <see cref="Microsoft.Extensions.Configuration.IConfiguration">IConfiguration</see>
        /// used to provide logging configuration values from various configuration sources.
        /// </param>
        /// <param name="defaultLifestyle">Default Lifestyle with which to create loggers</param>
        /// <param name="shouldLogToConsole">Optional: Determines whether to pipe all events through the console. <c>true</c> by default.</param>
        /// <param name="excludedLoggerTypes">Optional: Types to exclude as log sources</param>
        /// <returns>
        /// The <paramref name="container">
        /// Container</paramref> being used as the composition root
        /// of your application.
        /// </returns>
        public static Container AddEventLogging(
            this Container container,
            IConfiguration frameworkConfiguration,
            Lifestyle defaultLifestyle,
            bool shouldLogToConsole = true,
            params Type[] excludedLoggerTypes
        )
        {
            new LoggingModule(
                frameworkConfiguration,
                defaultLifestyle,
                shouldLogToConsole,
                excludedLoggerTypes
            ).RegisterServices(container);

            return container;
        }
    }
}