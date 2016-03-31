using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    public static class HealthCheckExtensions
    {
        public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder app,
                                                         PathString healthPath = default(PathString),
                                                         Func<HttpContext, int> checkHealthStatus = null)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            if (!healthPath.HasValue)
            {
                healthPath = "/_health";
            }

            if (checkHealthStatus == null)
            {
                checkHealthStatus = _ => 200;
            }

            app.Map(healthPath,
                subApp => subApp.Run(
                    context =>
                    {
                        context.Response.StatusCode = checkHealthStatus.Invoke(context);
                        return Task.FromResult(0);
                    }
                )
            );

            return app;
        }
    }
}
