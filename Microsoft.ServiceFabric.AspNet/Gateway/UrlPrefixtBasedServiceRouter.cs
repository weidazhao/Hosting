using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class UrlPrefixtBasedServiceRouter : ServiceRouter
    {
        public UrlPrefixtBasedServiceRouter(IServiceDescription serviceDescription)
            : base(serviceDescription)
        {
        }

        public override Task<bool> CanRouteRequestAsync(HttpRequestMessage request)
        {
            var requestUriBuilder = new UriBuilder(request.RequestUri);
            var requestUriPathSegments = requestUriBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var serviceName = new Uri($"fabric:/{requestUriPathSegments[0]}/{requestUriPathSegments[1]}", UriKind.Absolute);

            bool canRouteRequest = requestUriPathSegments.Length >= 2 && ServiceDescription.ServiceName == serviceName;

            return Task.FromResult(canRouteRequest);
        }

        public override Task RouteRequestAsync(HttpRequestMessage request, Uri serviceEndpoint)
        {
            var requestUriBuilder = new UriBuilder(request.RequestUri);
            var requestUriPathSegments = requestUriBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            requestUriBuilder.Path = string.Join("/", requestUriPathSegments.Skip(2));
            request.RequestUri = requestUriBuilder.Uri;

            return base.RouteRequestAsync(request, serviceEndpoint);
        }
    }
}
