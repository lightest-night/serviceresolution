using LightestNight.ServiceResolution;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ServiceResolution.Tests
{
    public class ServiceFactoryTests
    {
        private class TestObject
        {
            public string Property { get; }= nameof(TestObject);
                
            public TestObject(){}

            public TestObject(string property)
            {
                Property = property;
            }
        }
        
        public class Activator
        {
            private readonly ServiceFactory _sut;

            public Activator()
            {
                _sut = System.Activator.CreateInstance;
            }
            
            [Fact]
            public void Should_Build_Object_Correctly_With_Parameterless_Constructor()
            {
                // Act
                var result = _sut(typeof(TestObject)) as TestObject;
                
                // Assert
                result?.Property.ShouldBe(nameof(TestObject));
            }

            [Fact]
            public void Should_Build_Object_With_Constructor_Parameters()
            {
                // Arrange
                const string property = "Test";
                
                // Act
                var result = _sut(typeof(TestObject), property) as TestObject;
                
                // Assert
                result?.Property.ShouldBe(property);
            }
        }
        
        public class ServiceCollection
        {
            private readonly ServiceFactory _sut;

            public ServiceCollection()
            {
                var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
                services.AddServiceResolution();

                _sut = services.BuildServiceProvider().GetRequiredService<ServiceFactory>();
            }

            [Fact]
            public void Should_Build_Object_Correctly_With_Parameterless_Constructor()
            {
                // Act
                var result = _sut(typeof(TestObject)) as TestObject;
                
                // Assert
                result?.Property.ShouldBe(nameof(TestObject));
            }
            
            [Fact]
            public void Should_Build_Object_With_Constructor_Parameters()
            {
                // Arrange
                const string property = "Test";
                
                // Act
                var result = _sut(typeof(TestObject), property) as TestObject;
                
                // Assert
                result?.Property.ShouldBe(property);
            }
        }
    }
}