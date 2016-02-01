using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class ServiceFabricOptions
    {
        public string EndpointName { get; set; }

        public Type ServiceType { get; set; }

        public Action<IServiceCollection> ConfigureServices { get; set; }
    }
}
