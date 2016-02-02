using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class StatefulServiceStartupFilter : IStartupFilter
    {
        private readonly ServiceFabricOptions _options;

        public StatefulServiceStartupFilter(ServiceFabricOptions options)
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
                builder.UseMiddleware<StatefulServiceMiddleware>(_options);

                next.Invoke(builder);
            };
        }
    }
}
