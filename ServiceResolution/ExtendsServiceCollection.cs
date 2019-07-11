using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LightestNight.System.Utilities.Extensions;
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

        /// <summary>
        /// Uses reflection to add all delegates exposed using the <see cref="IExposesDelegates{TExposer}" /> interface
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add the delegates to</param>
        /// <param name="assemblies">A collection of <see cref="Assembly" /> to search in</param>
        public static IServiceCollection AddExposedDelegates(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies.IsNullOrEmpty())
                assemblies = new[] {Assembly.GetCallingAssembly()};

            var executingAssembly = Assembly.GetExecutingAssembly();
            if (!assemblies.Contains(executingAssembly))
                assemblies = new List<Assembly>(assemblies) {executingAssembly}.ToArray();

            var exposesDelegatesType = typeof(IExposesDelegates<>);
            var serviceFactory = services.BuildServiceProvider().GetService<ServiceFactory>() ?? Activator.CreateInstance;
            Parallel.ForEach(assemblies, assembly =>
            {
                var exposesDelegatesImplementations = assembly
                    .GetTypes()
                    .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == exposesDelegatesType))
                    .ToArray();

                Parallel.ForEach(exposesDelegatesImplementations, imp =>
                {
                    var delegateExposerType = imp.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == exposesDelegatesType)
                        ?.GenericTypeArguments.FirstOrDefault();
                    if (delegateExposerType == default)
                        return;

                    var delegateExposer = (DelegateExposer) serviceFactory(delegateExposerType);
                    delegateExposer.ExposeDelegates(services);
                });
            });

            return services;
        }
    }
}