using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;
using System.ComponentModel.Design;
using Xunit;

namespace AspNetCore.ApplicationBlocks.Tests.Frontend
{
    public class MvcIntegrationExtensionsTests
    {
        [Fact]
        public void AddDefaultJsonConfiguration_Returns_Same_IMvcBuilder()
        {
            var builder = A.Fake<IMvcBuilder>();
            var outBuilder = MvcIntegrationExtensions.AddDefaultJsonConfiguration(builder);

            builder.Should().BeSameAs(outBuilder);
        }

        [Theory]
        [InlineData(typeof(IControllerActivator))]
        [InlineData(typeof(IViewComponentActivator))]
        public void IntegrateSimpleInjector_Registers_Expected_Types(Type expectedServiceType)
        {
            var container = new Container();
            var services = new ServiceCollection();
            services.IntegrateSimpleInjector(container);

            services.Should().Contain(service => service.ServiceType == expectedServiceType);
        }

        [Fact]
        public void ForFrontEnd_Sets_Correct_Scope_Lifecycle()
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();
            container.ForFrontEnd();

            container.Options.DefaultScopedLifestyle.Should().BeOfType<AsyncScopedLifestyle>();
        }

        [Fact]
        public void ConfigureForMvc_CrossWires_IHttpContextAccessor()
        {
            var container = new Container();
            container.ForFrontEnd();
            var services = new ServiceCollection();
            services.EnableSimpleInjectorCrossWiring(container);
            services.AddHttpContextAccessor();
            services.AddMvc();

            IApplicationBuilder applicationBuilder = new ApplicationBuilder(services.BuildServiceProvider());
            container.ConfigureForMvc(applicationBuilder);
            container.GetRegistration(typeof(IHttpContextAccessor)).Should().NotBeNull();
        }
    }
}
