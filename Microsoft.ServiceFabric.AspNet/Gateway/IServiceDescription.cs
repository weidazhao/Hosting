using System;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public interface IServiceDescription
    {
        Uri ServiceName { get; }

        ServicePartitionKind PartitionKind { get; }

        string ListenerName { get; }

        Task<string> ComputeNamedPartitionKeyAsync(HttpRequestMessage request);

        Task<long> ComputeUniformInt64PartitionKeyAsync(HttpRequestMessage request);
    }
}