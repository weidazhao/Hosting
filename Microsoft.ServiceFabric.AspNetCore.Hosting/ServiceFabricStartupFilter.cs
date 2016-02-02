using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class ServiceFabricStartupFilter : IStartupFilter
    {
        private readonly ServiceFabricOptions _options;

        public ServiceFabricStartupFilter(ServiceFabricOptions options)
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
                if (typeof(IStatelessServiceInstance).IsAssignableFrom(_options.ServiceType))
                {
                    if (_options.InterfaceTypes != null)
                    {
                        builder.UseMiddleware<StatelessServiceMiddleware>(_options);
                    }
                }
                else if (typeof(IStatefulServiceReplica).IsAssignableFrom(_options.ServiceType))
                {
                    builder.UseMiddleware<StatefulServiceMiddleware>(_options);
                }

                next.Invoke(builder);
            };
        }
    }
}
