using Microsoft.ServiceFabric.AspNetCore.Gateway;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpRequestDispatcherProvider(this IServiceCollection services, HttpRequestDispatcherProvider provider)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            services.AddSingleton(provider);

            return services;
        }

        public static IServiceCollection AddDefaultHttpRequestDispatcherProvider(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHttpRequestDispatcherProvider(new HttpRequestDispatcherProvider(null, new[] { new AlwaysTreatedAsNonTransientExceptionHandler() }, null));

            return services;
        }
    }
}
