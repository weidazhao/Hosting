using Microsoft.AspNet.Http;
using System;
using System.Fabric;
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

        public virtual Task<string> ComputeNamedPartitionKeyAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(context.Request.ToString());
        }

        public virtual Task<long> ComputeUniformInt64PartitionKeyAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<long>(context.Request.GetHashCode());
        }
    }
}
