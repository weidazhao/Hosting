using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore
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
                // Add ServiceFabricMiddleware to pipe line.
                //
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                services.AddTransient<IStartupFilter>(serviceProvider => new ServiceFabricStartupFilter(options));

                //
                // Add Service Fabric service to DI container.
                //
                if (options.ServiceType != null)
                {
                    services.AddScoped(options.ServiceType, serviceProvider =>
                    {
                        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                        var serviceFabricFeature = httpContextAccessor.HttpContext.Features.Get<ServiceFabricFeature>();

                        if (serviceFabricFeature != null && serviceFabricFeature.ServiceType == options.ServiceType)
                        {
                            return serviceFabricFeature.InstanceOrReplica;
                        }

                        return null;
                    });
                }

                //
                // Allow configuring other services.
                //
                if (options.ConfigureServices != null)
                {
                    options.ConfigureServices.Invoke(services);
                }
            });

            return webHostBuilder;
        }
    }
}
