using CorrelationId;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace AspNetCore.ApplicationBlocks.Tests.Frontend
{
    public class ServiceRegistrationExtensionsTests
    {
        [Theory]
        [InlineData(typeof(ICorrelationContextAccessor))]
        [InlineData(typeof(ICorrelationContextFactory))]
        public void AddCorrelationId_Registers_Expected_Services(Type expectedServiceType)
        {
            var container = new Container();
            container.AddCorrelationId();

            container.GetRegistration(expectedServiceType).ServiceType.Should().Be(expectedServiceType);
        }

        [Theory]
        [InlineData(typeof(ICorrelationContextAccessor))]
        [InlineData(typeof(ICorrelationContextFactory))]
        [InlineData(typeof(IHttpContextAccessor))]
        [InlineData(typeof(ILoggerFactory))]
        public void AddDefaultApiServices_Registers_Expected_Services(Type expectedServiceType)
        {
            var container = new Container();
            container.ForFrontEnd();
            var services = new ServiceCollection();
            services.AddDefaultApiServices(container, new HostingEnvironment());
            var provider = services.BuildServiceProvider();
            provider.GetService(expectedServiceType).Should().BeAssignableTo(expectedServiceType);
        }
    }
}
