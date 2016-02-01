using Microsoft.AspNetCore.Http;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Gateway
{
    public class SharedGateway
    {
        public static readonly SharedGateway Default = new SharedGateway();

        private readonly HttpClient _httpClient = new HttpClient();

        private readonly CommunicationClientFactory _communicationClientFactory = new CommunicationClientFactory();

        public async Task InvokeAsync(HttpContext context, GatewayOptions options)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            //
            // NOTE:
            // Some of the code is copied from https://github.com/AspNet/Proxy/blob/dev/src/Microsoft.AspNetCore.Proxy/ProxyMiddleware.cs for prototype purpose.
            // Reviewing the license of the code will be needed if this code is to be used in production.
            //

            var servicePartitionClient = await ResolveServicePartitionClientAsync(context, options);

            await servicePartitionClient.InvokeWithRetryAsync(async communicationClient =>
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
            });
        }

        private async Task<ServicePartitionClient<CommunicationClient>> ResolveServicePartitionClientAsync(HttpContext context, GatewayOptions options)
        {
            ServicePartitionClient<CommunicationClient> client = null;

            switch (options.ServiceDescription.PartitionKind)
            {
                case ServicePartitionKind.Singleton:
                    client = new ServicePartitionClient<CommunicationClient>(_communicationClientFactory, options.ServiceDescription.ServiceName);
                    break;

                case ServicePartitionKind.Int64Range:
                    long int64RangeKey = await options.ServiceDescription.ComputeUniformInt64PartitionKeyAsync(context);
                    client = new ServicePartitionClient<CommunicationClient>(_communicationClientFactory, options.ServiceDescription.ServiceName, int64RangeKey);
                    break;

                case ServicePartitionKind.Named:
                    string namedKey = await options.ServiceDescription.ComputeNamedPartitionKeyAsync(context);
                    client = new ServicePartitionClient<CommunicationClient>(_communicationClientFactory, options.ServiceDescription.ServiceName, namedKey);
                    break;

                default:
                    break;
            }

            client.ListenerName = options.ServiceDescription.ListenerName;

            return client;
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

            protected override bool OnHandleException(Exception ex, out ExceptionHandlingResult result)
            {
                //
                // TODO:
                // Analyze the given exception and return a proper result.

                result = new ExceptionHandlingRetryResult()
                {
                    ExceptionId = ex.GetType().GUID.ToString(),
                    IsTransient = false,
                    MaxRetryCount = 5,
                    RetryDelay = TimeSpan.FromSeconds(1)
                };

                return true;
            }
        }
    }
}
