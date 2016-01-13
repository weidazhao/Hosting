using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class HeaderBasedServiceRouter : ServiceRouterBase
    {
        public HeaderBasedServiceRouter(Uri serviceName, ServicePartitionKind partitionKind)
            : base(serviceName, partitionKind)
        {
        }

        public override Task<bool> CanRouteRequestAsync(HttpRequestMessage request)
        {
            bool canRouteRequest = false;

            IEnumerable<string> values;
            if (request.Headers.TryGetValues("SF-ServiceName", out values))
            {
                string value = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(value))
                {
                    Uri serviceName;
                    if (Uri.TryCreate(value, UriKind.Absolute, out serviceName))
                    {
                        canRouteRequest = ServiceName == serviceName;
                    }
                }
            }

            return Task.FromResult(canRouteRequest);
        }
    }
}
