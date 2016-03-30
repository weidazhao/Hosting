using Microsoft.ServiceFabric.AspNetCore.Gateway;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultHttpRequestDispatcherProvider(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton(_ => new HttpRequestDispatcherProvider(null, new[] { new AlwaysTreatedAsNonTransientExceptionHandler() }, null));

            return services;
        }
    }
}
