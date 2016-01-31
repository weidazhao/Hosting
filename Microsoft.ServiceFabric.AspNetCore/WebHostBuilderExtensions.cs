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
        public static IWebHostBuilder UseServiceFabric(this IWebHostBuilder webHostBuilder, string endpointName, Type serviceType = null)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            var endpoint = FabricRuntime.GetActivationContext().GetEndpoint(endpointName);

            string serverUrl = $"{endpoint.Protocol}://{FabricRuntime.GetNodeContext().IPAddressOrFQDN}:{endpoint.Port}";

            webHostBuilder.UseUrls(serverUrl);

            webHostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<IStartupFilter, ServiceFabricStartupFilter>();

                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                if (serviceType != null)
                {
                    services.AddScoped(serviceType, serviceProvider =>
                    {
                        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                        var serviceFabricFeature = httpContextAccessor.HttpContext.Features.Get<ServiceFabricFeature>();

                        var instanceOrReplica = serviceFabricFeature.InstanceOrReplica;

                        if (instanceOrReplica != null && serviceType.IsAssignableFrom(instanceOrReplica.GetType()))
                        {
                            return instanceOrReplica;
                        }

                        return null;
                    });
                }
            });

            return webHostBuilder;
        }
    }
}
