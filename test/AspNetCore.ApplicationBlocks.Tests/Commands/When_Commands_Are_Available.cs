using FakeItEasy;
using FluentAssertions;
using AspNetCore.ApplicationBlocks.Commands;
using AspNetCore.ApplicationBlocks.TestSupport.TestDependencyContext;
using AspNetCore.ApplicationBlocks.TestSupport.TestTransitiveDependencyContext;
using SerilogEventLogger;
using Xunit;

namespace AspNetCore.ApplicationBlocks.Tests.Commands
{
    public class When_Commands_Are_Available
    {
        private readonly ITestService mockService = A.Fake<ITestService>();

        private readonly ContainerTestFixture fixture = new ContainerTestFixture();

        public When_Commands_Are_Available()
        {
            fixture
                .WithRegistrations(container => container.RegisterInstance(mockService))
                .WithRegistrations(container => container.Register(typeof(IEventLogger<>), typeof(NullEventLogger<>)))
                .Container
                .AddCommands(discoveryContext: fixture.TestDependencyContext);
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
        public void All_Actions_Should_Be_Registered()
        {
            fixture
                .Container
                .GetInstance<IActionHandler<Action1>>()
                .Should()
                .NotBeNull();

            fixture
                .Container
                .GetInstance<IActionHandler<Action2>>()
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void All_Functions_Should_Be_Registered()
        {
            fixture
                .Container
                .GetInstance<IFunctionHandler<Function1, bool>>()
                .Should()
                .NotBeNull();

            fixture
                .Container
                .GetInstance<IFunctionHandler<Function2, bool>>()
                .Should()
                .NotBeNull();
        }

        [Fact]
        public void Router_Resolves_From_Container()
        {
            fixture
                .Container
                .GetInstance<ICommandRouter>()
                .ExecuteAction(new Action1());

            A
                .CallTo(() => mockService.DoSomething())
                .MustHaveHappenedOnceExactly();
        }
    }
}