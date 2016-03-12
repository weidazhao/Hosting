using System.Collections.Immutable;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class ServiceFabricOptions
    {
        public string EndpointName { get; set; }

        public ImmutableArray<ServiceDescription> ServiceDescriptions { get; set; }
    }
}
