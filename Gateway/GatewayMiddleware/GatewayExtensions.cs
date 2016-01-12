using Microsoft.ServiceFabric.AspNet.Gateway;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Builder
{
    public static class GatewayExtensions
    {
        public static IApplicationBuilder RunGateway(this IApplicationBuilder app, IEnumerable<IServiceRouter> serviceRouters)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (serviceRouters == null)
            {
                throw new ArgumentNullException(nameof(serviceRouters));
            }

            return app.UseMiddleware<GatewayMiddleware>(serviceRouters);
        }
    }
}
