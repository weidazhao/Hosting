using Microsoft.AspNetCore.Http;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;

namespace Microsoft.ServiceFabric.AspNetCore.Gateway
{
    public class GatewayOptions
    {
        public Uri ServiceUri { get; set; }

        public TargetReplicaSelector TargetReplicaSelector { get; set; }

        public string ListenerName { get; set; }

        public OperationRetrySettings OperationRetrySettings { get; set; }

        public Func<HttpContext, ServicePartitionKey> GetServicePartitionKey { get; set; }
    }
}
