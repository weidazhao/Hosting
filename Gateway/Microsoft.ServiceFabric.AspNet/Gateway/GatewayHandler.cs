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
        private readonly IServiceRouter[] _serviceRouters;

        public GatewayHandler(IEnumerable<IServiceRouter> serviceRouters)
        {
            if (serviceRouters == null)
            {
                throw new ArgumentNullException(nameof(serviceRouters));
            }

            _serviceRouters = serviceRouters.ToArray();
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

            foreach (var serviceRouter in _serviceRouters)
            {
                if (await serviceRouter.CanRouteRequestAsync(request))
                {
                    //
                    // Resolve partition and endpoint
                    //
                    ResolvedServicePartition partition = null;
                    ResolvedServiceEndpoint endpoint = null;

                    switch (serviceRouter.ServiceDescription.PartitionKind)
                    {
                        case ServicePartitionKind.Singleton:
                            partition = await resolver.ResolveAsync(serviceRouter.ServiceDescription.ServiceName, cancellationToken);
                            endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.Stateless);
                            break;

                        case ServicePartitionKind.Int64Range:
                            long int64RangeKey = await serviceRouter.ServiceDescription.ComputeUniformInt64PartitionKeyAsync(request);
                            partition = await resolver.ResolveAsync(serviceRouter.ServiceDescription.ServiceName, int64RangeKey, cancellationToken);
                            endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
                            break;

                        case ServicePartitionKind.Named:
                            string namedKey = await serviceRouter.ServiceDescription.ComputeNamedPartitionKeyAsync(request);
                            partition = await resolver.ResolveAsync(serviceRouter.ServiceDescription.ServiceName, namedKey, cancellationToken);
                            endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
                            break;

                        default:
                            break;
                    }

                    //
                    // Parse endpoint and route the request to it
                    //
                    var serviceAddress = JsonConvert.DeserializeObject<Address>(endpoint.Address);
                    var serviceEndpoint = new Uri(serviceAddress.Endpoints.First().Value, UriKind.Absolute);

                    await serviceRouter.RouteRequestAsync(request, serviceEndpoint);

                    return true;
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
