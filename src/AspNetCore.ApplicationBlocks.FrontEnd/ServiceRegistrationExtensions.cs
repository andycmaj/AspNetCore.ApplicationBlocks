using App.Metrics.Health;
using CorrelationId;
using AspNetCore.ApplicationBlocks.Configuration;
using AspNetCore.ApplicationBlocks.FrontEnd.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Serilog.AspNetCore;
using SimpleInjector;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;

namespace AspNetCore.ApplicationBlocks
{
    /// <summary>
    /// Helpers to add default services to ASPNETCORE and SimpleInjector DI
    /// </summary>
    public static class ServiceRegistrationExtensions
    {
        private static Action<SwaggerGenOptions> ConfigureSwaggerGenDefaults(Container container) =>
            options =>
            {
                var config = container.GetInstance<IApplicationConfiguration>();

                options.IncludeXmlComments(GetXmlCommentsPath(config.GetType().Namespace));
                options.DescribeAllEnumsAsStrings();
                options.DescribeStringEnumsInCamelCase();
                options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Title = $"{config.Application} API",
                    Version = "v1",
                    Description = $"Environment: '{config.Environment}' -- Version: '{config.Version}'",
                });
            };

        private static string GetXmlCommentsPath(string applicationName)
        {
            return Path.Combine(AppContext.BaseDirectory, $"{applicationName}.xml");
        }

        /// <summary>
        /// Configure the default set of UI components.
        /// </summary>
        /// <remarks>
        /// * AspNetCore MVC
        /// * Swagger API Spec Generation
        /// * Integrated React dev server for SPA development
        /// </remarks>
        /// <example>
        /// <code>
        /// // Default swaggerOptions
        /// options =>
        ///  {
        ///      options.DescribeAllEnumsAsStrings();
        ///      options.DescribeStringEnumsInCamelCase();
        ///      options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
        ///      {
        ///          Title = "HTTP API",
        ///          Version = "v1",
        ///          Description = "The Service HTTP API",
        ///      });
        ///  }
        /// </code>
        /// </example>
        /// <param name="services">The framework <see cref="IServiceCollection" /></param>
        /// <param name="container">The application composition root</param>
        /// <param name="env">
        /// The framework <see cref="IHostingEnvironment"/> used to
        /// determine whether to enable environment-specific middleware
        /// </param>
        /// <param name="useSpa">
        /// Optional: Specify `true` to enable SPA Razor host page and static file configuration.
        /// (Default: `true`)
        /// </param>
        /// <param name="spaStaticFilesRoot">
        /// Optional: The directory, relative to the FrontEnd project, where the SPA release content
        /// artifacts are placed after SPA release builds. This is the path from which the release SPA
        /// bundle will be served in 'production-like' environments.
        /// (Default: `ClientApp/build`)
        /// </param>
        /// <param name="useSwagger">
        /// Optional: Specify `true` to enable Swagger API Spec generation using Swashbuckle.AspNetCore.
        /// (Default: `true`)
        /// </param>
        /// <param name="swaggerOptions">
        /// Optional: An <c>Action</c> of type <see cref="SwaggerGenOptions" /> used to configure
        /// Swagger spec generation.
        /// (Default: see Remarks for default action)
        /// </param>
        /// <param name="configureHealthCheck">
        /// Optional: An <c>Func</c> of type <see cref="Func{IHealthBuilder, IHealthBuilder}" /> used to configure
        /// health checks.</param>
        /// <returns>The <see cref="IMvcBuilder" /> used to configure Mvc</returns>
        public static IMvcBuilder AddDefaultUiServices(
            this IServiceCollection services,
            Container container,
            IHostingEnvironment env,
            bool useSpa = true,
            string spaStaticFilesRoot = "ClientApp/build",
            bool useSwagger = true,
            Action<SwaggerGenOptions> swaggerOptions = null,
            Func<IHealthBuilder, IHealthBuilder> configureHealthCheck = null)
        {
            var mvcBuilder = AddDefaultApiServices(services, container, env, useSwagger, swaggerOptions, configureHealthCheck);

            if (useSpa)
            {
                mvcBuilder
                    .AddRazorOptions(options =>
                    {
                        // Add ViewLocation with layout view built by `npm run build` in admin-ui.
                        // This view will include the release javascript modules build by webpack.
                        options.ViewLocationFormats.Clear();
                        if (env.IsDevelopment())
                        {
                            // webpack dev server path
                            options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
                        }
                        else
                        {
                            options.ViewLocationFormats.Add("/Views/{1}/{0}.release.cshtml");
                        }
                    });

                // In production, the React files will be served from this directory
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = spaStaticFilesRoot;
                });
            }

            return mvcBuilder;
        }

        /// <summary>
        /// Adds the default set of API components.
        /// </summary>
        /// <remarks>
        /// * AspNetCore MVC
        /// * Swagger API Spec Generation
        /// </remarks>
        /// <example>
        /// <code>
        /// // Default swaggerOptions
        /// options =>
        ///  {
        ///      options.DescribeAllEnumsAsStrings();
        ///      options.DescribeStringEnumsInCamelCase();
        ///      options.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
        ///      {
        ///          Title = "HTTP API",
        ///          Version = "v1",
        ///          Description = "The Service HTTP API",
        ///      });
        ///  }
        /// </code>
        /// </example>
        /// <param name="services">The framework <see cref="IServiceCollection" /></param>
        /// <param name="container">The application composition root</param>
        /// <param name="env">
        /// The framework <see cref="IHostingEnvironment"/> used to
        /// determine whether to enable environment-specific middleware
        /// </param>
        /// <param name="useSwagger">
        /// Optional: Specify `true` to enable Swagger API Spec generation using Swashbuckle.AspNetCore.
        /// (Default: `true`)
        /// </param>
        /// <param name="swaggerOptions">
        /// Optional: An <c>Action</c> of type <see cref="Action{SwaggerGenOptions}" /> used to configure
        /// Swagger spec generation.
        /// (Default: see Remarks for default action)
        /// </param>
        /// <param name="configureHealthCheck">
        /// Optional: An <c>Func</c> of type <see cref="Func{IHealthBuilder, IHealthBuilder}" /> used to configure
        /// health checks.</param>
        /// <returns>The <see cref="IMvcBuilder" /> used to configure Mvc</returns>
        public static IMvcBuilder AddDefaultApiServices(
            this IServiceCollection services,
            Container container,
            IHostingEnvironment env,
            bool useSwagger = true,
            Action<SwaggerGenOptions> swaggerOptions = null,
            Func<IHealthBuilder, IHealthBuilder> configureHealthCheck = null
        )
        {
            // Redirect framework logging to Serilog logger
            services.AddSingleton<ILoggerFactory>(_ => new SerilogLoggerFactory(null, true));

            container.Register<RequestLoggingMiddleware>(Lifestyle.Scoped);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            if (env.IsDevelopment())
            {
                // https://andrewlock.net/understanding-your-middleware-pipeline-with-the-middleware-analysis-package/
                services.AddMiddlewareAnalysis();
            }

            container.AddCorrelationId();
            services.AddCorrelationId();

            if (useSwagger)
            {
                services.AddSwaggerGen(swaggerOptions ?? ConfigureSwaggerGenDefaults(container));
            }

            configureHealthCheck = configureHealthCheck ?? (o => o);
            configureHealthCheck(AppMetricsHealth.CreateDefaultBuilder()
                .HealthChecks.RegisterFromAssembly(services, DependencyContext.Default))
                .BuildAndAddTo(services);
            services.AddHealthEndpoints();

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            return services
                .AddMvc()
                .AddDefaultJsonConfiguration();
        }

        public static void AddCorrelationId(this Container container)
        {
            container.RegisterSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            container.Register<ICorrelationContextFactory>(() =>
                new CorrelationContextFactory(container.GetInstance<ICorrelationContextAccessor>())
            );
        }
    }
}