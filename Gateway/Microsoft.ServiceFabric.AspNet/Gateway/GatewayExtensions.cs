using Microsoft.AspNet.Builder;
using System;
using System.Collections.Generic;

namespace Microsoft.ServiceFabric.AspNet.Gateway
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
