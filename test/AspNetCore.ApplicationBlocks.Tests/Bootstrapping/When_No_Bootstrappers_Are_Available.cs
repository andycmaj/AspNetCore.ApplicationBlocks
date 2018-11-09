using SerilogEventLogger;
using Xunit;
using IBootstrapper = AspNetCore.ApplicationBlocks.Bootstrapping.IBootstrapper;
using AspNetCore.ApplicationBlocks.Bootstrapping;

namespace AspNetCore.ApplicationBlocks.Tests.Bootstrapping
{
    public class When_No_Bootstrappers_Are_Available
    {
        private readonly ContainerTestFixture fixture = new ContainerTestFixture();

        [Fact]
        public void Discovery_Does_Not_Fail()
        {
            fixture
                .Container
                .AddBootstrappers(fixture.EmptyDependencyContext);

            fixture.VerifyContainer();
        }

        [Fact]
        public void RunBootstrappers_Does_Not_Fail()
        {
            fixture
                .WithRegistrations(container => container.Register<IEventLogger<IBootstrapper>, NullEventLogger<IBootstrapper>>())
                .VerifyContainer()
                .Container
                .RunBootstrappers();
        }
    }
}