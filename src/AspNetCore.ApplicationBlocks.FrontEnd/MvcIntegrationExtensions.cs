using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;
using static AspNetCore.ApplicationBlocks.JsonConfiguration;

namespace AspNetCore.ApplicationBlocks
{
    public static class MvcIntegrationExtensions
    {
        /// <summary>
        /// Configure a Container for use in a FrontEnd application.
        /// </summary>
        public static Container ForFrontEnd(this Container container)
        {
            if (container.Options?.DefaultScopedLifestyle?.GetType() != typeof(AsyncScopedLifestyle))
            {
                try
                {
                    container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException($"FrontEnd requires the scope to be {nameof(AsyncScopedLifestyle)}", ex);
                }
            }

            return container;
        }

        /// <summary>
        /// Integrates SimpleInjector's <see cref="Container">DI Container</see> with AspNetCore's
        /// framework. As a result, AspNetCore will use the <see cref="Container" /> to resolve all
        /// Controller, View, Middleware, etc. dependencies.
        /// </summary>
        /// <remarks>
        /// See <a href="http://simpleinjector.readthedocs.io/en/latest/aspnetintegration.html">
        /// SimpleInjector's AspNetCore integration docs</a> for more info.
        /// </remarks>
        /// <param name="services">The framework <see cref="IServiceCollection" /></param>
        /// <param name="container">The SimpleInjector <see cref="Container" /></param>
        /// <returns>The framework <see cref="IServiceCollection" /></returns>
        public static IServiceCollection IntegrateSimpleInjector(
            this IServiceCollection services,
            Container container
        )
        {
            services.AddSingleton(container);

            // SimpleInjector integration ()
            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(container));
            services.AddSingleton<IViewComponentActivator>(new SimpleInjectorViewComponentActivator(container));

            services.UseSimpleInjectorAspNetRequestScoping(container);
            services.EnableSimpleInjectorCrossWiring(container);

            return services;
        }

        /// <summary>
        /// Configure the SimpleInjector Container for use with ASPNETCORE MVC Controllers and Views
        /// </summary>
        /// <param name="container"></param>
        /// <param name="app"></param>
        /// <returns></returns>
        public static Container ConfigureForMvc(this Container container, IApplicationBuilder app)
        {
            // Add application presentation components:
            container.RegisterMvcControllers(app);
            container.RegisterMvcViewComponents(app);

            // Cross-wire ASP.NET services (if any). For instance:
            // Controllers are activated by SimpleInjector, so all
            // ASPNETCORE DI services must be available to Container
            container.CrossWire<IHttpContextAccessor>(app);

            return container;
        }

        public static IMvcBuilder AddDefaultJsonConfiguration(this IMvcBuilder builder)
        {
            return builder.AddJsonOptions(options =>
            {
                var mvcSettings = options.SerializerSettings;
                mvcSettings.NullValueHandling = DefaultSerializerSettings.NullValueHandling;
                mvcSettings.MissingMemberHandling = DefaultSerializerSettings.MissingMemberHandling;
                mvcSettings.DefaultValueHandling = DefaultSerializerSettings.DefaultValueHandling;
                mvcSettings.ContractResolver = DefaultSerializerSettings.ContractResolver;
                mvcSettings.Converters = DefaultSerializerSettings.Converters;
            });
        }
    }
}