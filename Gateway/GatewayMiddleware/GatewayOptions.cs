using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class GatewayOptions
    {
        public Func<HttpRequestMessage, Uri, Task<string>> ComputeNamedPartitionKeyAsync { get; set; }

        public Func<HttpRequestMessage, Uri, Task<long>> ComputeUniformInt64PartitionKeyAsync { get; set; }
    }
}
