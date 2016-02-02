using Microsoft.AspNetCore.Http;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class StatelessServiceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServiceFabricOptions _options;

        public StatelessServiceMiddleware(RequestDelegate next, ServiceFabricOptions options)
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

            IStatelessServiceInstance instance;
            if (StatelessServiceInstanceCache.Default.TryGet(out instance))
            {
                context.Features.Set(new StatelessServiceFeature { Instance = instance });
            }

            await _next.Invoke(context);
        }
    }
}
