using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AspNetCore.ApplicationBlocks.Configuration
{
    /// <summary>
    /// General configuration applicable to any AspNetCore application.
    /// Configuration values are provided by <see cref="IHostingEnvironment" />
    /// and <see cref="IConfiguration" /> initialized by your AspNetCore
    /// hosting context.
    /// </summary>
    public class ApplicationConfiguration : IApplicationConfiguration
    {
        internal const string ApplicationNameKey = "Application:Name";
        internal const string ApplicationVersionKey = "Application:Version";
        internal const string ApplicationHostKey = "Application:Host";
        internal const string ProductionLikeEnvironmentsKey = "Application:ProductionLikeEnvironments";

        private readonly string[] productionLikeEnvironments;

        /// <value>
        /// The framework <see cref="IHostingEnvironment" /> used to provide environment
        /// context.
        /// </value>
        protected IHostingEnvironment HostingEnvironment { get; }

        /// <value>
        /// The framework <see cref="IConfiguration" /> used to provide configuration
        /// values.
        /// </value>
        protected IConfiguration ConfigRoot { get; }

        /// <summary>
        /// Create an instance of <c>ApplicationConfiguration</c> given framework-initialized
        /// configuration contexts.
        /// </summary>
        /// <param name="hostingEnvironment">
        /// The <see cref="IHostingEnvironment" /> containing information about the current environment.
        /// </param>
        /// <param name="configRoot">
        /// The framework <see cref="IConfiguration" /> which provides configuration values from multiple sources.
        /// </param>
        public ApplicationConfiguration(IHostingEnvironment hostingEnvironment, IConfiguration configRoot)
        {
            HostingEnvironment = hostingEnvironment;
            ConfigRoot = configRoot;

            productionLikeEnvironments =
                GetString(ProductionLikeEnvironmentsKey, EnvironmentName.Production)
                .ToLower()
                .Split(',')
                .Select(envName => envName.Trim())
                .ToArray();
        }

        /// <inheritdoc />
        public string Environment => HostingEnvironment.EnvironmentName;

        /// <inheritdoc />
        public string Application => GetString(ApplicationNameKey);

        /// <inheritdoc />
        public string Version => GetString(ApplicationVersionKey);

        /// <inheritdoc />
        public string Hostname => GetString(ApplicationHostKey, System.Environment.MachineName);

        /// <inheritdoc />
        public bool IsProduction() => productionLikeEnvironments.Contains(Environment.ToLower());

        /// <inheritdoc />
        public bool IsDevelopment() => !IsProduction();

        /// <summary>
        /// Get the specified configuration key as a <c>string</c>.
        /// </summary>
        /// <param name="key">The configuration key, ie. <c>"Database:ConnectionString"</c></param>
        /// <param name="default">
        /// A default <c>string</c> to use if the key is not found in the current configuration sources.
        /// </param>
        /// <returns>The configuration value or the <paramref name="default"/></returns>
        protected string GetString(string key, string @default = null)
        {
            var stringValue = ConfigRoot[key];
            if (string.IsNullOrEmpty(stringValue) && @default is null)
            {
                throw new ArgumentNullException(key, "Config value not found.");
            }

            return string.IsNullOrEmpty(stringValue) ? @default : stringValue;
        }

        /// <summary>
        /// Get the specified configuration key as a <c>bool</c>.
        /// </summary>
        /// <param name="key">The configuration key, ie. <c>"Authentication:IsAuth0Enabled"</c></param>
        /// <param name="default">
        /// A default <c>bool</c> to use if the key is not found in the current configuration sources.
        /// </param>
        /// <returns>The configuration value or the <paramref name="default"/></returns>
        protected bool GetBoolean(string key, bool @default = false)
        {
            var stringValue = GetString(key, @default.ToString());

            if (!bool.TryParse(stringValue, out bool value))
            {
                value = @default;
            }

            return value;
        }

        /// <summary>
        /// Get the specified configuration key as an <c>int</c>.
        /// </summary>
        /// <param name="key">The configuration key, ie. <c>"Caching:TimeToLiveInHours"</c></param>
        /// <param name="default">
        /// A default <c>int</c> to use if the key is not found in the current configuration sources.
        /// </param>
        /// <returns>The configuration value or the <paramref name="default"/></returns>
        protected int GetInt(string key, int? @default)
        {
            var stringValue = GetString(key, @default?.ToString());
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                throw new ArgumentNullException(key, "Config value not found.");
            }

            return int.Parse(stringValue);
        }
        /// <summary>
        /// Get the specified configuration key as an <c>Array</c> of <typeparamref name="T" />.
        /// </summary>
        /// <param name="key">The configuration key, ie. <c>"Releases:ActiveReleases"</c></param>
        /// <param name="default">
        /// A default <c>Array</c> of <typeparamref name="T" /> to use if the key is not found in
        /// the current configuration sources.
        /// </param>
        /// <returns>The configuration value or the <paramref name="default"/></returns>
        protected T[] GetArray<T>(string key, T[] @default = default(T[]))
        {
            var stringValue = GetString(key, JsonConvert.SerializeObject(@default));
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                throw new ArgumentNullException(key, "Config value not found.");
            }

            return JsonConvert.DeserializeObject<T[]>(stringValue);
        }

        /// <summary>
        /// Get the specified configuration key as a <c>Uri</c>.
        /// </summary>
        /// <param name="key">The configuration key, ie. <c>"Chunnel:EndpointUrl"</c></param>
        /// <param name="default">
        /// A default <c>Uri</c> to use if the key is not found in the current configuration sources.
        /// </param>
        /// <returns>The configuration value or the <paramref name="default"/></returns>
        protected Uri GetUri(string key, Uri @default = null)
        {
            var stringValue = GetString(key, @default?.ToString());
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                throw new ArgumentNullException(key, "Config value not found.");
            }

            if (!Uri.TryCreate(stringValue, UriKind.Absolute, out Uri uri))
            {
                throw new ArgumentException("Config value format (absolute uri) error.", key);
            }

            return uri;
        }
    }
}
