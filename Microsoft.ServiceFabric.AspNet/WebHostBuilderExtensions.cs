using Microsoft.AspNet.Hosting;
using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNet
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseServiceFabricEndpoint(this IWebHostBuilder webHostBuilder, ServiceInitializationParameters parameters, string endpointName)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            var endpoint = parameters.CodePackageActivationContext.GetEndpoint(endpointName);

            string serverUrl = $"{endpoint.Protocol}://{FabricRuntime.GetNodeContext().IPAddressOrFQDN}:{endpoint.Port}/{Guid.NewGuid()}";

            return webHostBuilder.UseSetting(WebHostDefaults.ServerUrlsKey, serverUrl);
        }
    }
}
