using AspNetCore.ApplicationBlocks.Logging;
using FluentAssertions;
using SerilogEventLogger;
using SimpleInjector;
using Xunit;

namespace AspNetCore.ApplicationBlocks.Tests.Logging
{
    public class LoggingModuleTest
    {
        private readonly ContainerTestFixture fixture = new ContainerTestFixture();

        public LoggingModuleTest()
        {
            fixture
                .WithApplicationConfiguration()
                .Container
                .AddEventLogging(Lifestyle.Singleton);
        }

        [Fact]
        public void Can_Resolve_EventLoggerOfT()
        {
            var logger = fixture.Container.GetInstance<IEventLogger<LoggingModuleTest>>();

            logger.Should().NotBeNull();
        }
    }
}