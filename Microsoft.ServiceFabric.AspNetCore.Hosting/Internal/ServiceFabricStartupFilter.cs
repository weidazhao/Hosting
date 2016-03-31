using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class ServiceFabricStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return builder =>
            {
                builder.UseMiddleware<ServiceFabricMiddleware>();

                next.Invoke(builder);
            };
        }
    }
}
