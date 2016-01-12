using Microsoft.ServiceFabric.AspNet.Gateway;
using System;

namespace Microsoft.AspNet.Builder
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
