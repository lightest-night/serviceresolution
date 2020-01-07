using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace LightestNight.System.ServiceResolution.Tests
{
    public class AssemblyScanningTestsFixture
    {
        private static Assembly Assembly => typeof(AssemblyScanningTestsFixture).Assembly;

        public class ClosedTypeTests
        {
            private interface IClosedInterface
            {
                string Property { get; }
            }

            private class ClosedClass : IClosedInterface
            {
                public string Property { get; } = string.Empty;
            }

            [Fact]
            public void Should_Register_ClosedClass_Against_IClosedInterface_Successfully()
            {
                // Arrange
                var services = new ServiceCollection();
                var resolutions = new[]
                {
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IClosedInterface)
                    }
                };
                
                // Act
                AssemblyScanning.RegisterServices(services, new []{Assembly}, resolutions);
                
                // Assert
                services.BuildServiceProvider().GetRequiredService<IClosedInterface>().ShouldBeOfType<ClosedClass>();
            }
            
            [Fact]
            public void Should_Only_Register_One_Instance_When_AddIfAlreadyExists_Is_False()
            {
                // Arrange
                var services = new ServiceCollection();
                var resolutions = new[]
                {
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IClosedInterface)
                    },
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IClosedInterface)
                    }
                };

                // Act
                AssemblyScanning.RegisterServices(services, new[] {Assembly}, resolutions);

                // Assert
                var registrations = services.BuildServiceProvider().GetServices<IClosedInterface>()
                    .ToArray();
                registrations.Length.ShouldBe(1);
            }

            [Fact]
            public void Should_Register_Multiple_Instances_When_AddIfAlreadyExists_Is_True()
            {
                // Arrange
                var services = new ServiceCollection();
                var resolutions = new[]
                {
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IClosedInterface)
                    },
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IClosedInterface),
                        AddIfAlreadyExists = true
                    }
                };

                // Act
                AssemblyScanning.RegisterServices(services, new[] {Assembly}, resolutions);

                // Assert
                var registrations = services.BuildServiceProvider().GetServices<IClosedInterface>()
                    .ToArray();
                registrations.Length.ShouldBe(2);
            }
        }

        public class OpenTypeTests
        {
            private interface IOpenSingularInterface<out T>
            {
                T Property { get; }
            }

            private interface IOpenManyInterface<in TIn, out TOut>
            {
                TOut Method(TIn arg);
            }

            private class OpenSingularClass : IOpenSingularInterface<string>
            {
                public string Property { get; } = nameof(OpenSingularClass);
            }

            private class OpenManyClass : IOpenManyInterface<object, string>
            {
                public string Method(object arg) => arg?.ToString() ?? string.Empty;
            }

            [Fact]
            public void Should_Register_OpenSingularClass_Against_IOpenSingularInterface_Successfully()
            {
                // Arrange
                var services = new ServiceCollection();
                var resolutions = new[]
                {
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IOpenSingularInterface<>)
                    }
                };

                // Act
                AssemblyScanning.RegisterServices(services, new[] {Assembly}, resolutions);

                // Assert
                services.BuildServiceProvider().GetRequiredService<IOpenSingularInterface<string>>()
                    .ShouldBeOfType<OpenSingularClass>();
            }

            [Fact]
            public void Should_Register_OpenManyClass_Against_IOpenManyInterface_Successfully()
            {
                // Arrange
                var services = new ServiceCollection();
                var resolutions = new[]
                {
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IOpenManyInterface<,>)
                    }
                };

                // Act
                AssemblyScanning.RegisterServices(services, new[] {Assembly}, resolutions);

                // Assert
                services.BuildServiceProvider().GetRequiredService<IOpenManyInterface<object, string>>()
                    .ShouldBeOfType<OpenManyClass>();
            }

            [Fact]
            public void Should_Only_Register_One_Instance_When_AddIfAlreadyExists_Is_False()
            {
                // Arrange
                var services = new ServiceCollection();
                var resolutions = new[]
                {
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IOpenSingularInterface<>)
                    },
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IOpenSingularInterface<>)
                    }
                };

                // Act
                AssemblyScanning.RegisterServices(services, new[] {Assembly}, resolutions);

                // Assert
                var registrations = services.BuildServiceProvider().GetServices<IOpenSingularInterface<string>>()
                    .ToArray();
                registrations.Length.ShouldBe(1);
            }

            [Fact]
            public void Should_Register_Multiple_Instances_When_AddIfAlreadyExists_Is_True()
            {
                // Arrange
                var services = new ServiceCollection();
                var resolutions = new[]
                {
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IOpenSingularInterface<>)
                    },
                    new ConcreteRegistration
                    {
                        InterfaceType = typeof(IOpenSingularInterface<>),
                        AddIfAlreadyExists = true
                    }
                };

                // Act
                AssemblyScanning.RegisterServices(services, new[] {Assembly}, resolutions);

                // Assert
                var registrations = services.BuildServiceProvider().GetServices<IOpenSingularInterface<string>>()
                    .ToArray();
                registrations.Length.ShouldBe(2);
            }
        }
    }
}