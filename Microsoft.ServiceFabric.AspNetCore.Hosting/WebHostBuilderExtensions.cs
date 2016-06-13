using System;
using System.Fabric;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseServiceFabricEndpoint(this IWebHostBuilder webHostBuilder, string endpointName)
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

            string serverUrl = $"{endpoint.Protocol.ToString().ToLowerInvariant()}://+:{endpoint.Port}";

            webHostBuilder.UseUrls(serverUrl);

            return webHostBuilder;
        }
    }
}
