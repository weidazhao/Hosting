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
        private readonly Func<HttpRequestMessage, long> _computePartitionKey;

        public GatewayHttpClientHandler(Uri serviceName)
            : this(serviceName, null)
        {
        }

        public GatewayHttpClientHandler(Uri serviceName, Func<HttpRequestMessage, long> computePartitionKey)
        {
            _serviceName = serviceName;
            _computePartitionKey = computePartitionKey;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resolver = ServicePartitionResolver.GetDefault();

            ResolvedServiceEndpoint endpoint = null;
            if (_computePartitionKey != null)
            {
                var partition = await resolver.ResolveAsync(_serviceName, _computePartitionKey(request), cancellationToken);

                endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
            }
            else
            {
                var partition = await resolver.ResolveAsync(_serviceName, cancellationToken);

                endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.Stateless);
            }

            dynamic address = JsonConvert.DeserializeObject(endpoint.Address);
            string urlString = address.Endpoints[""];
            Uri url = new Uri(urlString, UriKind.Absolute);
            request.RequestUri = ReplaceUri(request.RequestUri, url.Scheme, url.Host, url.Port);

            return await base.SendAsync(request, cancellationToken);
        }

        private static Uri ReplaceUri(Uri uri, string scheme, string host, int port)
        {
            UriBuilder builder = new UriBuilder(uri);

            builder.Scheme = scheme;
            builder.Host = host;
            builder.Port = port;

            return builder.Uri;
        }
    }
}
