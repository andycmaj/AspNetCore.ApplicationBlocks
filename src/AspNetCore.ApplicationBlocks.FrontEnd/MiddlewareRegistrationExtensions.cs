using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CorrelationId;
using GlobalExceptionHandler.WebApi;
using AspNetCore.ApplicationBlocks.FrontEnd.Middleware;
using SerilogEventLogger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SimpleInjector;

namespace AspNetCore.ApplicationBlocks
{
    /// <summary>
    /// Helpers to add default set of ASPNETCORE Middleware
    /// </summary>
    public static class MiddlewareRegistrationExtensions
    {
        private static Action<ISpaBuilder> ConfigureSpaDefaults =
            spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                spa.UseReactDevelopmentServer("start");
            };

        /// <summary>
        /// Adds the default set of UI middleware.
        /// </summary>
        /// <remarks>
        /// * Developer exception page
        /// * SSL termination support with X-Forwarded-Proto header
        /// * Swagger UI dashboard
        /// * Built-in React dev server
        /// * AspNetCore authentication
        /// * Request localization
        /// * Default AspNetCore routing
        /// </remarks>
        /// <example>
        /// <code>
        /// // Default configureSpa:
        /// spa =>
        ///   {
        ///       spa.Options.SourcePath = "ClientApp";
        ///       spa.UseReactDevelopmentServer("start");
        ///   }
        /// </code>
        /// </example>
        /// <param name="app">The framework <see cref="IApplicationBuilder"/></param>
        /// <param name="container">The application composition root</param>
        /// <param name="env">
        /// The framework <see cref="IHostingEnvironment"/> used to
        /// determine whether to enable environment-specific middleware
        /// </param>
        /// <param name="diagnosticListener">The DiagnosticListener used by the Framework</param>
        /// <param name="useSwagger">
        /// Optional: Specify `true` to enable Swagger dashboard UI at `/swagger`.
        /// (Default: `true`)
        /// </param>
        /// <param name="useSpa">
        /// Optional: Specify `true` to enable React SPA dev server in development mode.
        /// (Default: `true`)
        /// </param>
        /// <param name="useAuthentication">
        /// Optional: Specify `true` to enable AspNetCore Authentication.
        /// (Default: `true`)
        /// </param>
        /// <param name="configureSpa">
        /// Optional: An <c>Action</c> of type <see cref="ISpaBuilder" /> used configure the
        /// React SPA dev server.
        /// (Default: see remarks for default action)
        /// </param>
        /// <param name="localizationOptions">
        /// Optional: Specify <see cref="RequestLocalizationOptions" /> if your MVC application requires
        /// localization.
        /// </param>
        /// <returns>The framework <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseDefaultUiMiddleware(
            this IApplicationBuilder app,
            Container container,
            IHostingEnvironment env,
            DiagnosticListener diagnosticListener = null,
            bool useSwagger = true,
            bool useSpa = true,
            bool useAuthentication = true,
            Action<ISpaBuilder> configureSpa = null,
            RequestLocalizationOptions localizationOptions = null
        )
        {
            // Super important that this is first, otherwise all the middleware that is registered after this point
            // will have the incorrect IP address.
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
            });

            var isDevelopment = env.IsDevelopment();
            var isProduction = env.IsProduction();

            if (isDevelopment && diagnosticListener != null)
            {
                // https://andrewlock.net/understanding-your-middleware-pipeline-with-the-middleware-analysis-package/
                diagnosticListener.LogMiddlewareDiagnosticsToConsole();
            }

            app.Properties["analysis.NextMiddlewareName"] = nameof(CorrelationIdMiddleware);
            app.UseCorrelationId();
            // app.Use((c, next) => container.GetInstance<SerilogEnricherMiddleware>().Invoke(c, next));
            app.Properties["analysis.NextMiddlewareName"] = nameof(RequestLoggingMiddleware);
            app.Use((c, next) => container.GetInstance<RequestLoggingMiddleware>().Invoke(c, next));

            if (!isProduction)
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();

            app.UseGlobalExceptionHandler(x => {
                x.ContentType = "application/json";
                if (!isProduction)
                {
                    x.ResponseBody(e => JsonConvert.SerializeObject(new
                    {
                        message = e.Message,
                        exception = e.ToString()
                    }));
                }
                else // if production do not include stack
                {
                    x.ResponseBody(e => JsonConvert.SerializeObject(new
                    {
                        message = e.Message,
                    }));
                }
                x.OnError((exception, httpContext) => {
                    var logger = container.GetInstance<IEventLogger<ExceptionHandlerConfiguration>>();
                    logger.AlertEvent("UnhandledApiException", exception);
                    return Task.CompletedTask;
                });
            });

            app.UseHealthAllEndpoints();

            if (useSwagger)
            {
                app
                    .UseSwagger()
                    .UseSwaggerUI(options =>
                    {
                        options.SwaggerEndpoint("/swagger/v1/swagger.json", "HTTP API V1");
                    });
            }

            app.UseStaticFiles();

            if (useSpa)
            {
                app.UseSpaStaticFiles();
            }

            if (useAuthentication)
            {
                app.UseAuthentication();
            }

            // Localization needs to be after auth in case it reads from user info
            if (localizationOptions != null)
            {
                app.UseRequestLocalization(localizationOptions);
            }

            app.UseMvcWithDefaultRoute();

            if (useSpa && isDevelopment)
            {
                // Only configure React dev server if in Development
                app.MapWhen(IsSpaRoute, spaApp => {
                    // Only configure React dev server if in Development
                    UseSpaWithoutIndexHtml(spaApp, configureSpa ?? ConfigureSpaDefaults);
                });
            }

            return app;
        }

        /// <summary>
        /// Adds the default set of UI middleware.
        /// </summary>
        /// <remarks>
        /// * Developer exception page
        /// * SSL termination support with X-Forwarded-Proto header
        /// * Swagger UI dashboard
        /// * AspNetCore authentication
        /// * Default AspNetCore routing
        /// </remarks>
        /// <param name="app">The framework <see cref="IApplicationBuilder"/></param>
        /// <param name="container">The application composition root</param>
        /// <param name="env">
        /// The framework <see cref="IHostingEnvironment"/> used to
        /// determine whether to enable environment-specific middleware
        /// </param>
        /// <param name="diagnosticListener">The DiagnosticListener used by the Framework</param>
        /// <param name="useAuthentication">
        /// Optional: Specify `true` to enable AspNetCore Authentication.
        /// (Default: `true`)
        /// </param>
        /// <param name="useSwagger"></param>
        /// <returns>The framework <see cref="IApplicationBuilder"/></returns>
        public static IApplicationBuilder UseDefaultApiMiddleware(
            this IApplicationBuilder app,
            Container container,
            IHostingEnvironment env,
            DiagnosticListener diagnosticListener = null,
            bool useAuthentication = true,
            bool useSwagger = true
        )
        {
            return UseDefaultUiMiddleware(
                app,
                container,
                env,
                diagnosticListener,
                useSwagger,
                useSpa: false,
                useAuthentication: true
            );
        }

        private static bool IsSpaRoute(HttpContext context)
        {
            var path = context.Request.Path;
            return path.StartsWithSegments("/static")
                || path.StartsWithSegments("/sockjs-node")
                || path.StartsWithSegments("/socket.io")
                || path.ToString().Contains(".hot-update.");
        }

        private static bool IsApiRoute(HttpContext context)
        {
            var path = context.Request.Path;
            return path.StartsWithSegments("/api");
        }

        private static void UseSpaWithoutIndexHtml(IApplicationBuilder app, Action<ISpaBuilder> configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Use the options configured in DI (or blank if none was configured). We have to clone it
            // otherwise if you have multiple UseSpa calls, their configurations would interfere with one another.
            var optionsProvider = app.ApplicationServices.GetService<IOptions<SpaOptions>>();
            var options = new SpaOptions();

            var spaBuilder = new DefaultSpaBuilder(app, options);
            configuration.Invoke(spaBuilder);
        }

        private class DefaultSpaBuilder : ISpaBuilder
        {
            public IApplicationBuilder ApplicationBuilder { get; }

            public SpaOptions Options { get; }

            public DefaultSpaBuilder(IApplicationBuilder applicationBuilder, SpaOptions options)
            {
                ApplicationBuilder = applicationBuilder
                    ?? throw new ArgumentNullException(nameof(applicationBuilder));

                Options = options
                    ?? throw new ArgumentNullException(nameof(options));
            }
        }
    }
}