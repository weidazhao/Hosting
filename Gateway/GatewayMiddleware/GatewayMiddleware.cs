using Microsoft.AspNet.Http;
using Microsoft.AspNet.Proxy;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class GatewayMiddleware
    {
        private readonly ProxyMiddleware _proxyMiddleware;

        public GatewayMiddleware(RequestDelegate next, IEnumerable<IServiceRequestRouter> routers)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (routers == null)
            {
                throw new ArgumentNullException(nameof(routers));
            }

            var proxyOptions = new ProxyOptions()
            {
                Host = "must_not_be_empty",
                BackChannelMessageHandler = new GatewayHandler(routers)
            };

            _proxyMiddleware = new ProxyMiddleware(next, proxyOptions);
        }

        public Task Invoke(HttpContext context)
        {
            return _proxyMiddleware.Invoke(context);
        }
    }
}
