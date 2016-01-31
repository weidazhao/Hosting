using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore
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

            PathString remainingPath;
            object instanceOrReplica;

            if (UrlPrefixRegistry.Default.StartWithUrlPrefix(context.Request.Path, out remainingPath, out instanceOrReplica))
            {
                context.Request.Path = remainingPath;
                context.Features.Set(new ServiceFabricFeature { InstanceOrReplica = instanceOrReplica });
            }

            await _next.Invoke(context);
        }
    }
}
