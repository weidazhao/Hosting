using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Microsoft.ServiceFabric.AspNetCore
{
    public class ServiceRepoStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<ServiceRepoMiddleware>();
                next.Invoke(builder);
            };
        }
    }
}
