using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyModel;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace AspNetCore.ApplicationBlocks.Tests
{
    public class ContainerTestFixture
    {
        public Container Container { get; }

        public DependencyContext TestDependencyContext { get; }

        public DependencyContext EmptyDependencyContext { get; }

        public ContainerTestFixture()
        {
            TestDependencyContext = DependencyContext.Default;
            EmptyDependencyContext = new DependencyContext(
                new TargetInfo("netcoreapp2.0", string.Empty, string.Empty, true),
                CompilationOptions.Default,
                new CompilationLibrary[0],
                new RuntimeLibrary[0],
                new RuntimeFallbacks[0]
            );

            Container = new Container();
            Container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        }

        public ContainerTestFixture WithRegistrations(Action<Container> register)
        {
            register(Container);

            return this;
        }

        public ContainerTestFixture VerifyContainer()
        {
            Container.Verify();

            return this;
        }

        public ContainerTestFixture ShouldHaveRegistrationsFor<TService>(int expectedRegistrationCount)
            where TService : class
        {
            Container.GetAllInstances<TService>().Should().HaveCount(expectedRegistrationCount);

            return this;
        }

        public ContainerTestFixture ShouldHaveRegistrationFor<TService>()
        {
            Container.GetRegistration(typeof(TService), true);

            return this;
        }
    }
}