using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace Microsoft.ServiceFabric.AspNetCore.Gateway
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

            return app.UseMiddleware<GatewayMiddleware>(Options.Create(options));
        }
    }
}
