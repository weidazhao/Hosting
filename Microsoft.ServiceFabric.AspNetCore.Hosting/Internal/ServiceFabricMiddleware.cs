using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class ServiceFabricMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceFabricOptions _options;

        public ServiceFabricMiddleware(RequestDelegate next, ServiceFabricOptions options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _options = options;
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
