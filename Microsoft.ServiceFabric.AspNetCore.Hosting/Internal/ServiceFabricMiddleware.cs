using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class ServiceFabricMiddleware
    {
        private readonly RequestDelegate _next;

        public ServiceFabricMiddleware(RequestDelegate next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            _next = next;
        }

        public async Task Invoke(HttpContext context, ServiceFabricServiceRegistry registry, ServiceFabricServiceScope scope)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            PathString urlPrefix;
            PathString remainingPath;
            object service;

            if (!registry.TryGet(context.Request.Path, out urlPrefix, out remainingPath, out service))
            {
                context.Response.StatusCode = 503;

                return;
            }

            context.Request.Path = remainingPath;
            context.Request.PathBase = context.Request.PathBase + urlPrefix;
            scope.Service = service;

            await _next.Invoke(context);
        }
    }
}
