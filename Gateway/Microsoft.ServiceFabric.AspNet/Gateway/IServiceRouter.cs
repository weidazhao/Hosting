using System;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public interface IServiceRouter
    {
        Uri ServiceName { get; }

        ServicePartitionKind PartitionKind { get; }

        Task<bool> CanRouteRequestAsync(HttpRequestMessage request);

        Task RouteRequestAsync(HttpRequestMessage request, Uri serviceEndpoint);

        Task<string> ComputeNamedPartitionKeyAsync(HttpRequestMessage request);

        Task<long> ComputeUniformInt64PartitionKeyAsync(HttpRequestMessage request);
    }
}
