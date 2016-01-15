using Microsoft.AspNet.Http;
using System;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class GatewayMiddleware
    {
        private readonly GatewayOptions _options;

        public GatewayMiddleware(RequestDelegate next, GatewayOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public Task Invoke(HttpContext context)
        {
            return SharedGateway.Default.InvokeAsync(context, _options);
        }
    }
}
