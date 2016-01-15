using Microsoft.AspNet.Http;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class SharedGateway
    {
        public static readonly SharedGateway Default = new SharedGateway();

        private readonly HttpClient _httpClient = new HttpClient();

        private readonly CommunicationClientFactory _communicationClientFactory = new CommunicationClientFactory();

        public async Task InvokeAsync(HttpContext context, GatewayOptions options)
        {
            //
            // NOTE:
            // Some of the code is copied from https://github.com/aspnet/Proxy/blob/dev/src/Microsoft.AspNet.Proxy/ProxyMiddleware.cs for prototype purpose.
            // Reviewing the license of the code will be needed if this code is to be used in production.
            //

            const int MaxRetry = 5;
            const int DelayInSeconds = 1;

            CommunicationClient communicationClient = null;

            for (int retry = 0; retry < MaxRetry; retry++)
            {
                try
                {
                    communicationClient = await ResolveCommunicationClientAsync(context, options.ServiceDescription, communicationClient);

                    if (communicationClient != null)
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
                        requestUriBuilder.Scheme = communicationClient.ResolvedServiceEndpoint.Scheme;
                        requestUriBuilder.Host = communicationClient.ResolvedServiceEndpoint.Host;
                        requestUriBuilder.Port = communicationClient.ResolvedServiceEndpoint.Port;
                        requestUriBuilder.Path = PathString.FromUriComponent(communicationClient.ResolvedServiceEndpoint) + context.Request.Path + context.Request.QueryString;

                        requestMessage.RequestUri = requestUriBuilder.Uri;

                        //
                        // Set host header
                        //
                        requestMessage.Headers.Host = communicationClient.ResolvedServiceEndpoint.Host + ":" + communicationClient.ResolvedServiceEndpoint.Port;

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
                        }

                        break;
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

        private async Task<CommunicationClient> ResolveCommunicationClientAsync(HttpContext context, IServiceDescription serviceDescription, CommunicationClient previous)
        {
            CommunicationClient current = null;

            if (previous == null)
            {
                switch (serviceDescription.PartitionKind)
                {
                    case ServicePartitionKind.Singleton:
                        current = await _communicationClientFactory.GetClientAsync(serviceDescription.ServiceName, serviceDescription.ListenerName, context.RequestAborted);
                        break;

                    case ServicePartitionKind.Int64Range:
                        long int64RangeKey = await serviceDescription.ComputeUniformInt64PartitionKeyAsync(context);
                        current = await _communicationClientFactory.GetClientAsync(serviceDescription.ServiceName, int64RangeKey, serviceDescription.ListenerName, context.RequestAborted);
                        break;

                    case ServicePartitionKind.Named:
                        string namedKey = await serviceDescription.ComputeNamedPartitionKeyAsync(context);
                        current = await _communicationClientFactory.GetClientAsync(serviceDescription.ServiceName, namedKey, serviceDescription.ListenerName, context.RequestAborted);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                current = await _communicationClientFactory.GetClientAsync(previous.ResolvedServicePartition, serviceDescription.ListenerName, context.RequestAborted);
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
