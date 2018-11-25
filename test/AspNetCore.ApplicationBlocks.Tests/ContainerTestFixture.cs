using System;
using AspNetCore.ApplicationBlocks.Configuration;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
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

        public ContainerTestFixture WithApplicationConfiguration(
            string environment = "testEnvironment",
            string application = "testApplication",
            string version = "testVersion",
            string hostName = "testHost"
        )
        {
            var mockConfig = A.Fake<IApplicationConfiguration>(config => config.ConfigureFake(
                fake => {
                    A.CallTo(() => fake.Environment).Returns(environment);
                    A.CallTo(() => fake.Application).Returns(application);
                    A.CallTo(() => fake.Version).Returns(version);
                    A.CallTo(() => fake.Hostname).Returns(hostName);
                }
            ));

            Container.RegisterInstance<IApplicationConfiguration>(mockConfig);

            return this;
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