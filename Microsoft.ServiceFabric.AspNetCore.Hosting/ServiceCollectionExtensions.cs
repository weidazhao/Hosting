using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting.Internal;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScopedStatelessService<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : StatelessService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddScopedService(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddScopedStatelessService<TService>(this IServiceCollection services)
            where TService : StatelessService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddScopedStatelessService<TService, TService>();
        }

        public static IServiceCollection AddScopedStatefulService<TService, TImplementation>(this IServiceCollection services)
            where TImplementation : StatefulService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddScopedService(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddScopedStatefulService<TService>(this IServiceCollection services)
            where TService : StatefulService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddScopedStatefulService<TService, TService>();
        }

        private static IServiceCollection AddScopedService(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            //
            // Adds ServiceFabricServiceRegistry if it has not been added yet.
            //
            if (!services.Any(p => p.ImplementationType == typeof(ServiceFabricServiceRegistry)))
            {
                services.AddSingleton<ServiceFabricServiceRegistry>();
            }

            //
            // Adds ServiceFabricServiceScope if it has not been added yet.
            //
            if (!services.Any(p => p.ImplementationType == typeof(ServiceFabricServiceScope)))
            {
                services.AddScoped<ServiceFabricServiceScope>();
            }

            //
            // Adds ServiceFabricStartupFilter if it has not been added yet.
            //
            if (!services.Any(p => p.ImplementationType == typeof(ServiceFabricStartupFilter)))
            {
                services.AddTransient<IStartupFilter, ServiceFabricStartupFilter>();
            }

            //
            // Adds the given Service Fabric service.
            //
            services.AddScoped(serviceType, requestServices =>
            {
                var service = requestServices.GetRequiredService<ServiceFabricServiceScope>().Service;

                return service?.GetType() == implementationType ? service : null;
            });

            return services;
        }
    }
}
