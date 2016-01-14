using Microsoft.AspNet.Builder;
using System;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public static class GatewayExtensions
    {
        public static IApplicationBuilder RunGateway(this IApplicationBuilder app, GatewayOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<GatewayMiddleware>(options);
        }
    }
}
