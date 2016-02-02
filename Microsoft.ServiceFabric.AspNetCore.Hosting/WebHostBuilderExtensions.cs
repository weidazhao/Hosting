using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
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

            webHostBuilder.ConfigureServices(services =>
            {
                //
                // Add ServiceFabricMiddleware to pipeline.
                //
                services.AddTransient<IStartupFilter>(serviceProvider => new ServiceFabricStartupFilter(options));

                //
                // Add Service Fabric service to DI container.
                //
                if (options.InterfaceTypes != null)
                {
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                    foreach (var interfaceType in options.InterfaceTypes)
                    {
                        services.AddScoped(interfaceType, serviceProvider =>
                        {
                            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                            var serviceFabricFeature = httpContextAccessor.HttpContext.Features.Get<ServiceFabricFeature>();

                            return serviceFabricFeature?.InstanceOrReplica;
                        });
                    }
                }

                //
                // Configure other services.
                //
                if (options.ConfigureServices != null)
                {
                    options.ConfigureServices(services);
                }
            });

            return webHostBuilder;
        }
    }
}
