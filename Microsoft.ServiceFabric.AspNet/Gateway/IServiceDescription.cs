using Microsoft.AspNet.Http;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public interface IServiceDescription
    {
        Uri ServiceName { get; }

        ServicePartitionKind PartitionKind { get; }

        string ListenerName { get; }

        Task<string> ComputeNamedPartitionKeyAsync(HttpContext context);

        Task<long> ComputeUniformInt64PartitionKeyAsync(HttpContext context);
    }
}