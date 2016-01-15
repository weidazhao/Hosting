using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class ServiceRouter : IRouter
    {
        private readonly IServiceDescription _serviceDescription;
        private readonly CommunicationClientFactory _clientFactory;
        private readonly HttpClient _httpClient;

        public ServiceRouter(IServiceDescription serviceDescription)
        {
            if (serviceDescription == null)
            {
                throw new ArgumentNullException(nameof(serviceDescription));
            }

            _serviceDescription = serviceDescription;
            _clientFactory = new CommunicationClientFactory();
            _httpClient = new HttpClient();
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        public virtual Task RouteAsync(RouteContext context)
        {
            context.Handler = InvokeAsync;

            return Task.FromResult(true);
        }

        protected virtual async Task InvokeAsync(HttpContext context)
        {
            //
            // NOTE:
            // Some of the code is copied from https://github.com/aspnet/Proxy/blob/dev/src/Microsoft.AspNet.Proxy/ProxyMiddleware.cs for prototype purpose.
            // Review the license of the code will be needed if this code is going to be used in production.
            //

            const int MaxRetry = 5;
            const int DelayInSeconds = 1;

            CommunicationClient client = null;

            for (int retry = 0; retry < MaxRetry; retry++)
            {
                try
                {
                    client = await ResolveCommunicationClientAsync(context.Request, client);

                    if (client != null)
                    {
                        var requestMessage = new HttpRequestMessage();

                        //
                        // Copy the request method
                        //
                        requestMessage.Method = new HttpMethod(context.Request.Method);

                        //
                        // Copy the request content
                        //
                        if (!StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "GET") &&
                            !StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "HEAD") &&
                            !StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "DELETE") &&
                            !StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "TRACE"))
                        {
                            requestMessage.Content = new StreamContent(context.Request.Body);
                        }

                        //
                        // Copy the request headers
                        //
                        foreach (var header in context.Request.Headers)
                        {
                            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                            {
                                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                            }
                        }

                        //
                        // Construct the request URL
                        //
                        var requestUriBuilder = new UriBuilder();
                        requestUriBuilder.Scheme = client.ResolvedServiceEndpoint.Scheme;
                        requestUriBuilder.Host = client.ResolvedServiceEndpoint.Host;
                        requestUriBuilder.Port = client.ResolvedServiceEndpoint.Port;
                        requestUriBuilder.Path = PathString.FromUriComponent(client.ResolvedServiceEndpoint) + context.Request.Path + context.Request.QueryString;

                        requestMessage.RequestUri = requestUriBuilder.Uri;

                        //
                        // Set host header
                        //
                        requestMessage.Headers.Host = client.ResolvedServiceEndpoint.Host + ":" + client.ResolvedServiceEndpoint.Port;

                        //
                        // Send request and copy the result back to HttpResponse
                        //
                        using (var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                        {
                            //
                            // Copy the status code
                            //
                            context.Response.StatusCode = (int)responseMessage.StatusCode;

                            //
                            // Copy the response headers
                            //
                            foreach (var header in responseMessage.Headers)
                            {
                                context.Response.Headers[header.Key] = header.Value.ToArray();
                            }

                            foreach (var header in responseMessage.Content.Headers)
                            {
                                context.Response.Headers[header.Key] = header.Value.ToArray();
                            }

                            // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
                            context.Response.Headers.Remove("transfer-encoding");

                            //
                            // Copy the response content
                            //
                            await responseMessage.Content.CopyToAsync(context.Response.Body);

                            break;
                        }
                    }
                }
                catch
                {
                    // TODO
                    // Analyze the exception and decide whether to retry
                }

                //
                // Wait for some time before retry
                //
                if (retry < MaxRetry - 1)
                {
                    await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds));
                }
            }
        }

        private async Task<CommunicationClient> ResolveCommunicationClientAsync(HttpRequest request, CommunicationClient previous)
        {
            CommunicationClient current = null;

            if (previous == null)
            {
                switch (_serviceDescription.PartitionKind)
                {
                    case ServicePartitionKind.Singleton:
                        current = await _clientFactory.GetClientAsync(_serviceDescription.ServiceName, _serviceDescription.ListenerName, default(CancellationToken));
                        break;

                    case ServicePartitionKind.Int64Range:
                        long int64RangeKey = await _serviceDescription.ComputeUniformInt64PartitionKeyAsync(request);
                        current = await _clientFactory.GetClientAsync(_serviceDescription.ServiceName, int64RangeKey, _serviceDescription.ListenerName, default(CancellationToken));
                        break;

                    case ServicePartitionKind.Named:
                        string namedKey = await _serviceDescription.ComputeNamedPartitionKeyAsync(request);
                        current = await _clientFactory.GetClientAsync(_serviceDescription.ServiceName, namedKey, _serviceDescription.ListenerName, default(CancellationToken));
                        break;

                    default:
                        break;
                }
            }
            else
            {
                current = await _clientFactory.GetClientAsync(previous.ResolvedServicePartition, _serviceDescription.ListenerName, default(CancellationToken));
            }

            return current;
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
