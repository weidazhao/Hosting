using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.AspNetCore.Hosting.Internal;
using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseServiceFabric(this IWebHostBuilder webHostBuilder, ServiceFabricOptions options)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            //
            // Configure server URL.
            //
            var endpoint = FabricRuntime.GetActivationContext().GetEndpoint(options.EndpointName);

            string serverUrl = $"{endpoint.Protocol}://{FabricRuntime.GetNodeContext().IPAddressOrFQDN}:{endpoint.Port}";

            webHostBuilder.UseUrls(serverUrl);

            //
            // Configure services and middlewares.
            //
            webHostBuilder.ConfigureServices(services =>
            {
                if (typeof(IStatelessServiceInstance).IsAssignableFrom(options.ServiceType) && options.InterfaceTypes != null)
                {
                    services.AddTransient<IStartupFilter>(serviceProvider => new StatelessServiceStartupFilter(options));

                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                    foreach (var interfaceType in options.InterfaceTypes)
                    {
                        services.AddScoped(interfaceType, serviceProvider =>
                        {
                            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                            var feature = httpContextAccessor.HttpContext.Features.Get<StatelessServiceFeature>();

                            return feature?.Instance;
                        });
                    }
                }
                else if (typeof(IStatefulServiceReplica).IsAssignableFrom(options.ServiceType))
                {
                    services.AddTransient<IStartupFilter>(serviceProvider => new StatefulServiceStartupFilter(options));

                    if (options.InterfaceTypes != null)
                    {
                        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                        foreach (var interfaceType in options.InterfaceTypes)
                        {
                            services.AddScoped(interfaceType, serviceProvider =>
                            {
                                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                                var feature = httpContextAccessor.HttpContext.Features.Get<StatefulServiceFeature>();

                                return feature?.Replica;
                            });
                        }
                    }
                }

                if (options.ConfigureServices != null)
                {
                    options.ConfigureServices(services);
                }
            });

            return webHostBuilder;
        }
    }
}
