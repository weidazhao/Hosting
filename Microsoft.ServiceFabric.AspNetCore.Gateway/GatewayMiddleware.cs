using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Gateway
{
    public class GatewayMiddleware
    {
        private readonly HttpRequestDispatcherProvider _dispatcherProvider;
        private readonly GatewayOptions _options;

        public GatewayMiddleware(RequestDelegate next, HttpRequestDispatcherProvider dispatcherProvider, IOptions<GatewayOptions> options)
        {
            if (dispatcherProvider == null)
            {
                throw new ArgumentNullException(nameof(dispatcherProvider));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _dispatcherProvider = dispatcherProvider;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            //
            // NOTE:
            // Some of the code is copied from https://github.com/AspNet/Proxy/blob/dev/src/Microsoft.AspNetCore.Proxy/ProxyMiddleware.cs for prototype purpose.
            // Reviewing the license of the code will be needed if this code is to be used in production.
            //
            var servicePartitionClient = new ServicePartitionClient<HttpRequestDispatcher>(_dispatcherProvider,
                                                                                           _options.ServiceUri,
                                                                                           _options.GetServicePartitionKey?.Invoke(context),
                                                                                           _options.TargetReplicaSelector,
                                                                                           _options.ListenerName,
                                                                                           _options.OperationRetrySettings);

            await servicePartitionClient.InvokeWithRetryAsync(async dispatcher =>
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
                        requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                //
                // Flow path base through the custom header X-ServiceFabric-PathBase.
                //
                requestMessage.Headers.TryAddWithoutValidation("X-ServiceFabric-PathBase", context.Request.PathBase);

                //
                // Construct the request URL
                //
                var baseAddress = dispatcher.BaseAddress;
                var pathAndQuery = PathString.FromUriComponent(baseAddress) + context.Request.Path + context.Request.QueryString;

                requestMessage.RequestUri = new Uri($"{baseAddress.Scheme}://{baseAddress.Host}:{baseAddress.Port}{pathAndQuery}", UriKind.Absolute);

                //
                // Set host header
                //
                requestMessage.Headers.Host = baseAddress.Host + ":" + baseAddress.Port;

                //
                // Send request and copy the result back to HttpResponse
                //
                using (var responseMessage = await dispatcher.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    //
                    // If the service is temporarily unavailable, throw to retry later.
                    //
                    if (responseMessage.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        responseMessage.EnsureSuccessStatusCode();
                    }

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
    }
}
