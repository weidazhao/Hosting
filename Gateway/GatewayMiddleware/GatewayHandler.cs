using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class GatewayHandler : HttpClientHandler
    {
        private readonly IServiceRequestRouter[] _routers;

        public GatewayHandler(IEnumerable<IServiceRequestRouter> routers)
        {
            if (routers == null)
            {
                throw new ArgumentNullException(nameof(routers));
            }

            _routers = routers.ToArray();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!await RouteRequestAsync(request, cancellationToken))
            {
                throw new InvalidOperationException();
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<bool> RouteRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resolver = new ServicePartitionResolver(() => new FabricClient());

            foreach (var router in _routers)
            {
                if (await router.CanRouteRequestAsync(request))
                {
                    ResolvedServicePartition partition = null;
                    ResolvedServiceEndpoint endpoint = null;

                    switch (router.PartitionKind)
                    {
                        case ServicePartitionKind.Singleton:
                            partition = await resolver.ResolveAsync(router.ServiceName, cancellationToken);
                            endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.Stateless);
                            break;

                        case ServicePartitionKind.Int64Range:
                            long int64RangeKey = await router.ComputeUniformInt64PartitionKeyAsync(request);
                            partition = await resolver.ResolveAsync(router.ServiceName, int64RangeKey, cancellationToken);
                            endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
                            break;

                        case ServicePartitionKind.Named:
                            string namedKey = await router.ComputeNamedPartitionKeyAsync(request);
                            partition = await resolver.ResolveAsync(router.ServiceName, namedKey, cancellationToken);
                            endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
                            break;

                        default:
                            break;
                    }

                    if (partition != null && endpoint != null)
                    {
                        var serviceAddress = JsonConvert.DeserializeObject<Address>(endpoint.Address);
                        var serviceEndpoint = new Uri(serviceAddress.Endpoints.First().Value, UriKind.Absolute);

                        await router.RouteRequestAsync(request, serviceEndpoint);

                        return true;
                    }
                }
            }

            return false;
        }

        private sealed class Address
        {
            public Dictionary<string, string> Endpoints { get; set; }
        }
    }
}
