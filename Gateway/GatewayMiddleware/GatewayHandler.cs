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
        private readonly GatewayOptions _options;

        public GatewayHandler(GatewayOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await RewriteRequestAsync(request, cancellationToken);

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task RewriteRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var resolver = new ServicePartitionResolver(() => new FabricClient());

            //
            // Extract service name from request URI path
            //
            var requestUriBuilder = new UriBuilder(request.RequestUri);

            var pathSegments = requestUriBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length < 2)
            {
                throw new InvalidOperationException();
            }

            Uri serviceName = new Uri($"fabric:/{pathSegments[0]}/{pathSegments[1]}", UriKind.Absolute);

            //
            // Resolve service endpoint
            //
            ResolvedServiceEndpoint endpoint = null;
            if (_options.ComputeUniformInt64PartitionKeyAsync != null)
            {
                long partitionKey = await _options.ComputeUniformInt64PartitionKeyAsync(request, serviceName);
                var partition = await resolver.ResolveAsync(serviceName, partitionKey, cancellationToken);
                endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
            }
            else if (_options.ComputeNamedPartitionKeyAsync != null)
            {
                string partitionKey = await _options.ComputeNamedPartitionKeyAsync(request, serviceName);
                var partition = await resolver.ResolveAsync(serviceName, partitionKey, cancellationToken);
                endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.StatefulPrimary);
            }
            else
            {
                var partition = await resolver.ResolveAsync(serviceName, cancellationToken);
                endpoint = partition.Endpoints.First(p => p.Role == ServiceEndpointRole.Stateless);
            }

            //
            // Parse the endpoint
            //
            var address = JsonConvert.DeserializeObject<Address>(endpoint.Address);
            string urlString = address.Endpoints.First().Value;
            Uri url = new Uri(urlString, UriKind.Absolute);

            //
            // Rewrite request
            //
            var builder = new UriBuilder(request.RequestUri)
            {
                Scheme = url.Scheme,
                Host = url.Host,
                Port = url.Port,
                Path = string.Join("/", pathSegments.Skip(2))
            };

            request.RequestUri = builder.Uri;
            request.Headers.Host = url.Host + ":" + url.Port;
        }

        private sealed class Address
        {
            public Dictionary<string, string> Endpoints { get; set; }
        }
    }
}
