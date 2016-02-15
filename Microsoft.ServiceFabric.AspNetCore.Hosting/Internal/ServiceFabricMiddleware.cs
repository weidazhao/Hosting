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

        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            PathString urlPrefix;
            PathString remainingPath;
            object instanceOrReplica;

            if (ServiceFabricRegistry.Default.TryGet(context.Request.Path, out urlPrefix, out remainingPath, out instanceOrReplica))
            {
                context.Request.Path = remainingPath;
                context.Request.PathBase = context.Request.PathBase + urlPrefix;
                context.Features.Set(new ServiceFabricFeature { InstanceOrReplica = instanceOrReplica });
            }

            await _next.Invoke(context);
        }
    }
}
