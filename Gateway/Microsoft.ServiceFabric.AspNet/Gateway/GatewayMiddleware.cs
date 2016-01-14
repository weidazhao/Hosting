using Microsoft.AspNet.Http;
using Microsoft.AspNet.Proxy;
using System;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class GatewayMiddleware
    {
        private readonly ProxyMiddleware _proxyMiddleware;

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

            var proxyOptions = new ProxyOptions()
            {
                Host = "must_not_be_empty",
                BackChannelMessageHandler = new GatewayHandler(options)
            };

            _proxyMiddleware = new ProxyMiddleware(next, proxyOptions);
        }

        public Task Invoke(HttpContext context)
        {
            var pathBase = context.Request.PathBase;
            context.Request.PathBase = PathString.Empty;

            try
            {
                return _proxyMiddleware.Invoke(context);
            }
            finally
            {
                context.Request.PathBase = pathBase;
            }
        }
    }
}
