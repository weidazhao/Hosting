using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting.Internal;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceFabricService<TService>(this IServiceCollection services)
        {
            return services.AddServiceFabricService(typeof(TService), typeof(TService));
        }

        public static IServiceCollection AddServiceFabricService<TService, TImplementation>(this IServiceCollection services)
        {
            return services.AddServiceFabricService(typeof(TService), typeof(TImplementation));
        }

        public static IServiceCollection AddServiceFabricService(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            if (!typeof(StatelessService).IsAssignableFrom(implementationType) && !typeof(StatefulService).IsAssignableFrom(implementationType))
            {
                throw new ArgumentException(null, nameof(implementationType));
            }

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
