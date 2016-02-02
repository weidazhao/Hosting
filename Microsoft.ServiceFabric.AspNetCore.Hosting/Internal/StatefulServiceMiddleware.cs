using Microsoft.AspNetCore.Http;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class StatefulServiceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceFabricOptions _options;

        public StatefulServiceMiddleware(RequestDelegate next, ServiceFabricOptions options)
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

            PathString replicaUrlPrefix;
            PathString remainingPath;
            IStatefulServiceReplica replica;

            if (StatefulServiceReplicaCache.Default.TryGet(context.Request.Path, out replicaUrlPrefix, out remainingPath, out replica))
            {
                context.Request.Path = remainingPath;
                context.Request.PathBase = context.Request.PathBase + replicaUrlPrefix;
                context.Features.Set(new StatefulServiceFeature { Replica = replica });
            }

            await _next.Invoke(context);
        }
    }
}
