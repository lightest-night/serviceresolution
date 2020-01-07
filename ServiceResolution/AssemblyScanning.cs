using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightestNight.System.Utilities.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LightestNight.System.ServiceResolution
{
    public static class AssemblyScanning
    {
        public static void RegisterServices(IServiceCollection services, IEnumerable<Assembly> assembliesToScan, IEnumerable<ConcreteRegistration> concreteRegistrations)
        {
            assembliesToScan = (assembliesToScan as Assembly[] ?? assembliesToScan).Distinct().ToArray();

            foreach (var concreteRegistration in concreteRegistrations)
            {
                ConnectImplementationsToTypesClosing(concreteRegistration, services, assembliesToScan);
                ConnectImplementationsToTypes(concreteRegistration, services, assembliesToScan);
            }
        }

        private static void ConnectImplementationsToTypes(ConcreteRegistration concreteRegistration,
            IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            var interfaceType = concreteRegistration.InterfaceType;
            var concretions = assembliesToScan.SelectMany(assembly => assembly.GetInstancesOfInterface(interfaceType))
                .Where(type => type.IsConcrete()).ToList();
            
            var exactMatches = concretions
                .Where(concretion => concretion.CanBeCastTo(interfaceType)).ToList();
            if (concreteRegistration.AddIfAlreadyExists)
            {
                foreach (var type in exactMatches)
                    services.AddTransient(interfaceType, type);
            }
            else
            {
                if (exactMatches.Count > 1)
                    exactMatches.RemoveAll(match => !IsMatchingWithInterface(match, interfaceType));
                
                foreach (var type in exactMatches)
                    services.TryAddTransient(interfaceType, type);
            }
        }

        private static void ConnectImplementationsToTypesClosing(ConcreteRegistration concreteRegistration,
            IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            var concretions = new List<Type>();
            var interfaces = new List<Type>();

            foreach (var type in assembliesToScan.SelectMany(assembly => assembly.DefinedTypes)
                .Where(type => !type.IsOpenGeneric()))
            {
                var interfaceTypes = type.FindInterfacesThatClose(concreteRegistration.InterfaceType).ToArray();
                if (!interfaceTypes.Any())
                    continue;

                if (type.IsConcrete())
                    concretions.Add(type);

                foreach (var interfaceType in interfaceTypes)
                    interfaces.Fill(interfaceType);
            }

            foreach (var @interface in interfaces)
            {
                var exactMatches = concretions.Where(concretion => concretion.CanBeCastTo(@interface)).ToList();
                if (concreteRegistration.AddIfAlreadyExists)
                {
                    foreach (var type in exactMatches)
                        services.AddTransient(@interface, type);
                }
                else
                {
                    if (exactMatches.Count > 1)
                        exactMatches.RemoveAll(match => !IsMatchingWithInterface(match, @interface));

                    foreach (var type in exactMatches)
                        services.TryAddTransient(@interface, type);
                }

                if (!@interface.IsOpenGeneric())
                    AddConcretionsThatCouldBeClosed(@interface, concretions, services);
            }
        }

        private static bool IsOpenGeneric(this Type type)
            => type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;

        private static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
            => FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();

        private static IEnumerable<Type> FindInterfacesThatClosesCore(this Type pluggedType, Type templateType)
        {
            if (pluggedType == null)
                yield break;

            if (!pluggedType.IsConcrete())
                yield break;

            if (templateType.GetTypeInfo().IsInterface)
                foreach (var interfaceType in pluggedType.GetInterfaces().Where(type =>
                    type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == templateType))
                    yield return interfaceType;
            else if (pluggedType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                     pluggedType.GetTypeInfo().BaseType?.GetGenericTypeDefinition() == templateType)
                yield return pluggedType.GetTypeInfo().BaseType;

            if (pluggedType.GetTypeInfo().BaseType == typeof(object))
                yield break;

            foreach (var interfaceType in FindInterfacesThatClosesCore(pluggedType.GetTypeInfo().BaseType, templateType))
                yield return interfaceType;
        }

        private static bool IsConcrete(this Type type)
            => !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;

        private static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value))
                return;

            list.Add(value);
        }

        private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null)
                return false;

            return pluggedType == pluginType || pluginType.GetTypeInfo().IsAssignableFrom(pluggedType.GetTypeInfo());
        }

        private static bool IsMatchingWithInterface(Type handlerType, Type handlerInterface)
        {
            while (true)
            {
                if (handlerType == null || handlerInterface == null) 
                    return false;

                if (handlerType.IsInterface)
                {
                    if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments)) 
                        return true;
                }
                else
                {
                    handlerType = handlerType.GetInterface(handlerInterface.Name);
                    continue;
                }

                return false;
            }
        }

        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions,
            IServiceCollection services)
        {
            foreach (var type in concretions.Where(concretion =>
                concretion.IsOpenGeneric() && concretion.CouldCloseTo(@interface)))
            {
                try
                {
                    services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                }
                catch (Exception)
                {
                    // Swallow exceptions so as not to bone the system, this will be graceful as the service will just not get registered and cause
                    // a non registration exception further up the stack
                }
            }
        }

        private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;
            var concreteArguments = openConcretion.GenericTypeArguments;

            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }
    }

    public struct ConcreteRegistration
    {
        public Type InterfaceType { get; set; }
        public bool AddIfAlreadyExists { get; set; }
    }
}