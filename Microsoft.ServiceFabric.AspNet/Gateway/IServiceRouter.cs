using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public interface IServiceRouter
    {
        IServiceDescription ServiceDescription { get; }

        Task<bool> CanRouteRequestAsync(HttpRequestMessage request);

        Task RouteRequestAsync(HttpRequestMessage request, Uri serviceEndpoint);
    }
}
