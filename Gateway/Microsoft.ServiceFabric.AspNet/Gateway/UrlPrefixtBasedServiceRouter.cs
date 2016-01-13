using System;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class UrlPrefixtBasedServiceRouter : ServiceRouterBase
    {
        public UrlPrefixtBasedServiceRouter(Uri serviceName, ServicePartitionKind partitionKind)
            : base(serviceName, partitionKind)
        {
        }

        public override Task<bool> CanRouteRequestAsync(HttpRequestMessage request)
        {
            var requestUriBuilder = new UriBuilder(request.RequestUri);
            var requestUriPathSegments = requestUriBuilder.Path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            bool canRouteRequest = requestUriPathSegments.Length >= 2 &&
                                   ServiceName == new Uri($"fabric:/{requestUriPathSegments[0]}/{requestUriPathSegments[1]}", UriKind.Absolute);

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
