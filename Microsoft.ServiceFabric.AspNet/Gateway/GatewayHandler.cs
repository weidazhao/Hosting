using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Fabric;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class GatewayHandler : HttpClientHandler
    {
        private readonly GatewayOptions _options;
        private readonly CommunicationClientFactory _clientFactory;

        public GatewayHandler(GatewayOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
            _clientFactory = new CommunicationClientFactory();
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
            foreach (var serviceRouter in _options.ServiceRouters)
            {
                if (await serviceRouter.CanRouteRequestAsync(request))
                {
                    CommunicationClient client = null;

                    switch (serviceRouter.ServiceDescription.PartitionKind)
                    {
                        case ServicePartitionKind.Singleton:
                            client = await _clientFactory.GetClientAsync(serviceRouter.ServiceDescription.ServiceName,
                                                                         serviceRouter.ServiceDescription.ListenerName,
                                                                         cancellationToken);
                            break;

                        case ServicePartitionKind.Int64Range:
                            long int64RangeKey = await serviceRouter.ServiceDescription.ComputeUniformInt64PartitionKeyAsync(request);
                            client = await _clientFactory.GetClientAsync(serviceRouter.ServiceDescription.ServiceName,
                                                                         int64RangeKey,
                                                                         serviceRouter.ServiceDescription.ListenerName,
                                                                         cancellationToken);
                            break;

                        case ServicePartitionKind.Named:
                            string namedKey = await serviceRouter.ServiceDescription.ComputeNamedPartitionKeyAsync(request);
                            client = await _clientFactory.GetClientAsync(serviceRouter.ServiceDescription.ServiceName,
                                                                         namedKey,
                                                                         serviceRouter.ServiceDescription.ListenerName,
                                                                         cancellationToken);
                            break;

                        default:
                            break;
                    }

                    await serviceRouter.RouteRequestAsync(request, client.ResolvedServiceEndpoint);

                    return true;
                }
            }

            return false;
        }

        private sealed class CommunicationClient : ICommunicationClient
        {
            public Uri ResolvedServiceEndpoint { get; set; }

            public ResolvedServicePartition ResolvedServicePartition { get; set; }
        }

        private sealed class CommunicationClientFactory : CommunicationClientFactoryBase<CommunicationClient>
        {
            protected override void AbortClient(CommunicationClient client)
            {
            }

            protected override Task<CommunicationClient> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
            {
                return Task.FromResult(new CommunicationClient() { ResolvedServiceEndpoint = new Uri(endpoint, UriKind.Absolute) });
            }

            protected override bool ValidateClient(CommunicationClient client)
            {
                return true;
            }

            protected override bool ValidateClient(string endpoint, CommunicationClient client)
            {
                return client.ResolvedServiceEndpoint == new Uri(endpoint, UriKind.Absolute);
            }
        }
    }
}
