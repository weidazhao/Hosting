using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class HeaderBasedServiceRouter : IServiceRouter
    {
        private readonly Uri _serviceName;
        private readonly ServicePartitionKind _partitionKind;

        public HeaderBasedServiceRouter(Uri serviceName, ServicePartitionKind partitionKind)
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

            _serviceName = serviceName;
            _partitionKind = partitionKind;
        }

        public Uri ServiceName
        {
            get { return _serviceName; }
        }

        public ServicePartitionKind PartitionKind
        {
            get { return _partitionKind; }
        }

        public Task<bool> CanRouteRequestAsync(HttpRequestMessage request)
        {
            bool canRouteRequest = false;

            IEnumerable<string> values;
            if (request.Headers.TryGetValues("SF-ServiceName", out values))
            {
                string value = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(value))
                {
                    Uri serviceName;
                    if (Uri.TryCreate(value, UriKind.Absolute, out serviceName))
                    {
                        canRouteRequest = _serviceName == serviceName;
                    }
                }
            }

            return Task.FromResult(canRouteRequest);
        }

        public Task RouteRequestAsync(HttpRequestMessage request, Uri serviceEndpoint)
        {
            var serviceEndpointBuilder = new UriBuilder(serviceEndpoint);
            var serviceEndpointPathSegments = serviceEndpointBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var requestUriBuilder = new UriBuilder(request.RequestUri);
            var requestUriPathSegments = requestUriBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            requestUriBuilder.Scheme = serviceEndpointBuilder.Scheme;
            requestUriBuilder.Host = serviceEndpointBuilder.Host;
            requestUriBuilder.Port = serviceEndpointBuilder.Port;
            requestUriBuilder.Path = string.Join("/", serviceEndpointPathSegments.Concat(requestUriPathSegments));

            request.RequestUri = requestUriBuilder.Uri;
            request.Headers.Host = serviceEndpointBuilder.Host + ":" + serviceEndpointBuilder.Port;

            return Task.FromResult(true);
        }

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
