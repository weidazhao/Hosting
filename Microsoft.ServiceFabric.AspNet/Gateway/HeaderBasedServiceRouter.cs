using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class HeaderBasedServiceRouter : ServiceRouter
    {
        private readonly string _headerName;

        public HeaderBasedServiceRouter(IServiceDescription serviceDescription, string headerName)
            : base(serviceDescription)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentException(null, nameof(headerName));
            }

            _headerName = headerName;
        }

        public override Task<bool> CanRouteRequestAsync(HttpRequestMessage request)
        {
            bool canRouteRequest = false;

            IEnumerable<string> values;
            if (request.Headers.TryGetValues(_headerName, out values))
            {
                string value = values.FirstOrDefault();
                if (!string.IsNullOrEmpty(value))
                {
                    Uri serviceName;
                    if (Uri.TryCreate(value, UriKind.Absolute, out serviceName))
                    {
                        canRouteRequest = ServiceDescription.ServiceName == serviceName;
                    }
                }
            }

            return Task.FromResult(canRouteRequest);
        }
    }
}
