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
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return SharedGateway.Default.InvokeAsync(context, _options);
        }
    }
}
