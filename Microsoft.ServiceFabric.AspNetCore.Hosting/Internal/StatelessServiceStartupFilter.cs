using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class StatelessServiceStartupFilter : IStartupFilter
    {
        private readonly ServiceFabricOptions _options;

        public StatelessServiceStartupFilter(ServiceFabricOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<StatelessServiceMiddleware>(_options);

                next.Invoke(builder);
            };
        }
    }
}
