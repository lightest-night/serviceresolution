using System;
using LightestNight.System.ServiceResolution;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ServiceResolution.Tests
{
    public class ExtendsServiceCollectionTests
    {
        private static class TestServiceFactory
        {
            public static object Create(Type type, params object[] args) => new object();
        }
        
        [Fact]
        public void Should_Return_The_Services_That_Are_Already_Populated()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ServiceFactory>(TestServiceFactory.Create);

            // Act
            serviceCollection.AddServiceResolution();
            
            // Assert
            serviceCollection.BuildServiceProvider().GetRequiredService<ServiceFactory>().Method.Name.ShouldBe(nameof(TestServiceFactory.Create));
        }

        [Fact]
        public void Should_Add_The_ActivatorUtilities()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            
            // Act
            serviceCollection.AddServiceResolution();
            
            // Assert
            serviceCollection.BuildServiceProvider().GetService<ServiceFactory>().ShouldNotBeNull();
        }
    }
}