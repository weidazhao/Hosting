using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
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
            scope.Service = service;

            StringValues pathBase;
            if (context.Request.Headers.TryGetValue("X-ServiceFabric-PathBase", out pathBase))
            {
                context.Request.PathBase = pathBase.FirstOrDefault();
            }

            await _next.Invoke(context);
        }
    }
}
