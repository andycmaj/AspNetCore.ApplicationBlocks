using System.Collections.Generic;
using System.Linq;
using Bogus;
using FluentAssertions;
using AspNetCore.ApplicationBlocks.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Iqviate.Templates.FrontEnd.Tests
{
    public class ApplicationConfigurationTest
    {
        private static ApplicationConfiguration Arrange(
            string environmentName,
            IEnumerable<KeyValuePair<string, string>> baseData,
            IEnumerable<KeyValuePair<string, string>> testData
        )
        {
            var hostingEnvironment = new HostingEnvironment
            {
                EnvironmentName = environmentName
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(baseData.ToDictionary(
                    item => item.Key,
                    item => item.Value
                ))
                .AddInMemoryCollection(testData.ToDictionary(
                    item => item.Key,
                    item => item.Value
                ))
                .Build();

            return new ApplicationConfiguration(hostingEnvironment, config);
        }

        public class Given_Application_Values
        {
            private const string Name = "appname";
            private const string Version = "appversion";
            private const string Host = "apphost";

            private readonly ApplicationConfiguration config =
                Arrange(
                    EnvironmentName.Development,
                    new Dictionary<string, string>
                    {
                        { ApplicationConfiguration.ApplicationNameKey, Name },
                        { ApplicationConfiguration.ApplicationVersionKey, Version },
                        { ApplicationConfiguration.ApplicationHostKey, Host },
                    },
                    new Dictionary<string, string>()
                );

            [Fact]
            public void ApplicationName_Matches_Config() =>
                config.Application.Should().Be(Name);

            [Fact]
            public void ApplicationVersion_Matches_Config() =>
                config.Version.Should().Be(Version);

            [Fact]
            public void ApplicationHost_Matches_Config() =>
                config.Hostname.Should().Be(Host);
        }

        public class Given_No_Explicit_ProductionLikeEnvironments
        {
            private static ApplicationConfiguration Arrange(
                string environmentName,
                params KeyValuePair<string, string>[] testData
            ) =>
                ApplicationConfigurationTest.Arrange(
                    environmentName,
                    new Dictionary<string, string>(),
                    testData
                );

            public class And_Given_Production_EnvironmentName
            {
                private readonly ApplicationConfiguration config =
                    Arrange(EnvironmentName.Production);

                [Fact]
                public void Environment_Should_Be_Production() =>
                    config.Environment.Should().Be(EnvironmentName.Production);

                [Fact]
                public void IsProduction_Should_Be_True() =>
                    config.IsProduction().Should().BeTrue();

                [Fact]
                public void IsDevelopment_Should_Be_False() =>
                    config.IsDevelopment().Should().BeFalse();
            }

            public class And_Given_Random_EnvironmentName
            {
                private readonly ApplicationConfiguration config =
                    Arrange(new Faker().Hacker.Noun());

                [Fact]
                public void IsProduction_Should_Be_False() =>
                    config.IsProduction().Should().BeFalse();

                [Fact]
                public void IsDevelopment_Should_Be_True() =>
                    config.IsDevelopment().Should().BeTrue();
            }
        }

        public class Given_Custom_ProductionLikeEnvironments
        {
            private const string PreProduction = "PreProduction";
            private const string Sales = "Sales";

            private static ApplicationConfiguration Arrange(
                string environmentName,
                params KeyValuePair<string, string>[] testData
            ) =>
                ApplicationConfigurationTest.Arrange(
                    environmentName,
                    new Dictionary<string, string>
                    {
                        {
                            ApplicationConfiguration.ProductionLikeEnvironmentsKey,
                            $"{PreProduction} , {Sales}"
                        }
                    },
                    testData
                );

            public class And_Given_ProductionLike_EnvironmentName
            {
                private readonly ApplicationConfiguration config =
                    Arrange(PreProduction);

                [Fact]
                public void Environment_Should_Be_PreProduction() =>
                    config.Environment.Should().Be(PreProduction);

                [Fact]
                public void IsProduction_Should_Be_True() =>
                    config.IsProduction().Should().BeTrue();

                [Fact]
                public void IsDevelopment_Should_Be_False() =>
                    config.IsDevelopment().Should().BeFalse();
            }

            public class And_Given_Random_EnvironmentName
            {
                private readonly ApplicationConfiguration config =
                    Arrange(new Faker().Hacker.Noun());

                [Fact]
                public void IsProduction_Should_Be_False() =>
                    config.IsProduction().Should().BeFalse();

                [Fact]
                public void IsDevelopment_Should_Be_True() =>
                    config.IsDevelopment().Should().BeTrue();
            }
        }
    }
}
