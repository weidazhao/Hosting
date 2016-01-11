using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using System;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway
{
    public class GatewayHttpClientHandler : HttpClientHandler
    {
        private readonly Uri _serviceName;
        private readonly Func<HttpRequestMessage, long> _computePartitionKeyAsLong;
        private readonly Func<HttpRequestMessage, string> _computePartitionKeyAsString;

        public GatewayHttpClientHandler(Uri serviceName)
        {
            _serviceName = serviceName;
        }

        public GatewayHttpClientHandler(Uri serviceName, Func<HttpRequestMessage, long> computePartitionKeyAsLong)
            : this(serviceName)
        {
            _computePartitionKeyAsLong = computePartitionKeyAsLong;
        }

        public GatewayHttpClientHandler(Uri serviceName, Func<HttpRequestMessage, string> computePartitionKeyAsString)
            : this(serviceName)
        {
            _computePartitionKeyAsString = computePartitionKeyAsString;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await RewriteRequestUriAsync(request, cancellationToken);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task RewriteRequestUriAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resolver = new ServicePartitionResolver(() => new FabricClient());

            //
            // Resolve service endpoint
            //
            ResolvedServiceEndpoint endpoint = null;
            if (_computePartitionKeyAsLong != null)
            {
                var partition = await resolver.ResolveAsync(_serviceName, _computePartitionKeyAsLong(request), cancellationToken);
                endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
            }
            else if (_computePartitionKeyAsString != null)
            {
                var partition = await resolver.ResolveAsync(_serviceName, _computePartitionKeyAsString(request), cancellationToken);
                endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
            }
            else
            {
                var partition = await resolver.ResolveAsync(_serviceName, cancellationToken);
                endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.Stateless);
            }

            //
            // Parse the endpoint
            //
            dynamic address = JsonConvert.DeserializeObject(endpoint.Address);
            string urlString = address.Endpoints[""];
            Uri url = new Uri(urlString, UriKind.Absolute);

            //
            // Rewrite request URL
            //
            var builder = new UriBuilder(request.RequestUri)
            {
                Scheme = url.Scheme,
                Host = url.Host,
                Port = url.Port
            };

            request.RequestUri = builder.Uri;
        }
    }
}
