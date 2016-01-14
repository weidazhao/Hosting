using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class ServiceRouter : IServiceRouter
    {
        public ServiceRouter(ServiceDescription serviceDescription)
        {
            if (serviceDescription == null)
            {
                throw new ArgumentNullException(nameof(serviceDescription));
            }

            ServiceDescription = serviceDescription;
        }

        public ServiceDescription ServiceDescription { get; }

        public virtual Task<bool> CanRouteRequestAsync(HttpRequestMessage request)
        {
            return Task.FromResult(true);
        }

        public virtual Task RouteRequestAsync(HttpRequestMessage request, Uri serviceEndpoint)
        {
            var serviceEndpointBuilder = new UriBuilder(serviceEndpoint);
            var serviceEndpointPathSegments = serviceEndpointBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var requestUriBuilder = new UriBuilder(request.RequestUri);
            var requestUriPathSegments = requestUriBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            requestUriBuilder.Scheme = serviceEndpointBuilder.Scheme;
            requestUriBuilder.Host = serviceEndpointBuilder.Host;
            requestUriBuilder.Port = serviceEndpointBuilder.Port;
            requestUriBuilder.Path = string.Join("/", serviceEndpointPathSegments.Concat(requestUriPathSegments));

            request.RequestUri = requestUriBuilder.Uri;
            request.Headers.Host = serviceEndpointBuilder.Host + ":" + serviceEndpointBuilder.Port;

            return Task.FromResult(true);
        }
    }
}
