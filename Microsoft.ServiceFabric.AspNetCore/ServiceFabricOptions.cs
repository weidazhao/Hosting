using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.ServiceFabric.AspNetCore
{
    public class ServiceFabricOptions
    {
        public ServiceFabricOptions(string endpointName)
        {
            if (endpointName == null)
            {
                throw new ArgumentNullException(nameof(endpointName));
            }

            EndpointName = endpointName;
        }

        public string EndpointName { get; }

        public Type ServiceType { get; set; }

        public Action<IServiceCollection> ConfigureServices { get; set; }
    }
}
