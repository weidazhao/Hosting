using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseServiceFabric(this IWebHostBuilder webHostBuilder, string endpointName)
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

            webHostBuilder.ConfigureServices(services => services.AddTransient<IStartupFilter, ServiceFabricStartupFilter>());

            return webHostBuilder;
        }
    }
}
