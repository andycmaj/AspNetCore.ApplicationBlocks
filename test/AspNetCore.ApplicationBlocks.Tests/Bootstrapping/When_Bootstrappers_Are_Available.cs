using FakeItEasy;
using IBootstrapper = AspNetCore.ApplicationBlocks.Bootstrapping.IBootstrapper;
using AspNetCore.ApplicationBlocks.TestSupport.TestDependencyContext;
using SerilogEventLogger;
using AspNetCore.ApplicationBlocks.Bootstrapping;
using Xunit;

namespace AspNetCore.ApplicationBlocks.Tests.Bootstrapping
{
    public class When_Bootstrappers_Are_Available
    {
        private readonly ITestService mockService = A.Fake<ITestService>();

        private readonly ContainerTestFixture fixture;

        public When_Bootstrappers_Are_Available()
        {
            fixture = new ContainerTestFixture()
                .WithRegistrations(container => container.Register<IEventLogger<IBootstrapper>, NullEventLogger<IBootstrapper>>())
                .WithRegistrations(container => container.RegisterInstance(mockService));

            fixture.Container.AddBootstrappers(fixture.TestDependencyContext);
        }

        [Fact]
        public void All_Bootstrappers_Are_Discovered()
        {
            fixture
                .VerifyContainer()
                .ShouldHaveRegistrationsFor<IBootstrapper>(5);
        }

        [Fact]
        public void Bootstrappers_Can_Be_Resolved_And_Executed()
        {
            fixture
                .VerifyContainer()
                .Container
                .RunBootstrappers();

            A.CallTo(() => mockService.DoSomething()).MustHaveHappenedTwiceExactly();
        }
    }
}