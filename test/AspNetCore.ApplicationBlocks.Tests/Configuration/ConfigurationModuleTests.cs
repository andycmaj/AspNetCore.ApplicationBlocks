using FluentAssertions;
using AspNetCore.ApplicationBlocks.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using SimpleInjector;
using Xunit;

namespace AspNetCore.ApplicationBlocks.Tests.Configuration
{
    public class ConfigurationModuleTests
    {
        [Fact]
        public void ConfigurationExtended()
        {
            var container = new Container();
            var configModule = new ConfigurationModule<IExtendedConfiguration, ExtendedConfiguration>(
                new ConfigurationBuilder().Build(), new HostingEnvironment());

            configModule.RegisterServices(container);

            var applicationConfig = container.GetInstance<IApplicationConfiguration>();
            applicationConfig.Should().BeOfType<ExtendedConfiguration>();
            container.GetInstance<IExtendedConfiguration>().Should().BeOfType<ExtendedConfiguration>().And.BeSameAs(applicationConfig);
        }

        [Fact]
        public void ApplicationConfiguration()
        {
            var container = new Container();
            var configModule = new ConfigurationModule<IApplicationConfiguration, ApplicationConfiguration>(
                new ConfigurationBuilder().Build(), new HostingEnvironment());

            configModule.RegisterServices(container);

            container.GetInstance<IApplicationConfiguration>().Should().BeOfType<ApplicationConfiguration>();
        }

        private interface IExtendedConfiguration : IApplicationConfiguration
        {
            string Foo { get; }
        }

        private class ExtendedConfiguration : ApplicationConfiguration, IExtendedConfiguration
        {
            private const string FooKey = "Foo";

            public ExtendedConfiguration(IHostingEnvironment hostingEnvironment, IConfiguration configRoot) :
                base(hostingEnvironment, configRoot)
            {
            }

            public string Foo => GetString(FooKey);
        }
    }
}
