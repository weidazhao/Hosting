using System;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class ServiceDescription : IServiceDescription
    {
        public ServiceDescription(Uri serviceName, ServicePartitionKind partitionKind, string listenerName = "")
        {
            if (serviceName == null)
            {
                throw new ArgumentNullException(nameof(serviceName));
            }

            if (partitionKind != ServicePartitionKind.Singleton &&
                partitionKind != ServicePartitionKind.Int64Range &&
                partitionKind != ServicePartitionKind.Named)
            {
                throw new ArgumentException(null, nameof(partitionKind));
            }

            if (listenerName == null)
            {
                throw new ArgumentNullException(nameof(listenerName));
            }

            ServiceName = serviceName;
            PartitionKind = partitionKind;
            ListenerName = listenerName;
        }

        public Uri ServiceName { get; }

        public ServicePartitionKind PartitionKind { get; }

        public string ListenerName { get; }

        public virtual Task<string> ComputeNamedPartitionKeyAsync(HttpRequestMessage request)
        {
            return Task.FromResult(request.ToString());
        }

        public virtual Task<long> ComputeUniformInt64PartitionKeyAsync(HttpRequestMessage request)
        {
            return Task.FromResult<long>(request.GetHashCode());
        }
    }
}
