using Microsoft.ServiceFabric.AspNet.Gateway;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Builder
{
    public static class GatewayExtensions
    {
        public static IApplicationBuilder RunGateway(this IApplicationBuilder app, IEnumerable<IServiceRequestRouter> routers)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (routers == null)
            {
                throw new ArgumentNullException(nameof(routers));
            }

            return app.UseMiddleware<GatewayMiddleware>(routers);
        }
    }
}
