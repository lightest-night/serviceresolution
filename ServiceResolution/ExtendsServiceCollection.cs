using Microsoft.Extensions.DependencyInjection;

namespace LightestNight.System.ServiceResolution
{
    public static class ExtendsServiceCollection
    {
        /// <summary>
        /// Adds the Microsoft Dependency Injection service provider as the <see cref="ServiceFactory" />
        /// </summary>
        public static IServiceCollection AddServiceResolution(this IServiceCollection services)
        {
            return services.BuildServiceProvider().GetService<ServiceFactory>() != null 
                ? services 
                : services.AddSingleton<ServiceFactory>(serviceProvider => (type, args) => ActivatorUtilities.CreateInstance(serviceProvider, type, args));
        }
    }
}