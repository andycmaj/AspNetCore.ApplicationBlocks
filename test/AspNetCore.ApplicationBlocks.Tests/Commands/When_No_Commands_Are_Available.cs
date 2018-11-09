using FakeItEasy;
using FluentAssertions;
using AspNetCore.ApplicationBlocks.Commands;
using AspNetCore.ApplicationBlocks.TestSupport.TestDependencyContext;
using SimpleInjector;
using Xunit;

namespace AspNetCore.ApplicationBlocks.Tests.Commands
{
    public class When_No_Commands_Are_Available
    {
        private readonly ContainerTestFixture fixture = new ContainerTestFixture();

        public When_No_Commands_Are_Available()
        {
            fixture
                .Container
                .AddCommands(discoveryContext: fixture.EmptyDependencyContext);
        }

        [Fact]
        public void Router_Is_Registered()
        {
            fixture
                .Container
                .GetInstance<ICommandRouter>()
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void No_Commands_Should_Be_Registered()
        {
            fixture
                .Container
                .Invoking(c => c.GetInstance<IActionHandler<Action1>>())
                .Should()
                .Throw<ActivationException>();
        }
    }
}