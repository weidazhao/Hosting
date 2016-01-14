using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ServiceFabric.AspNet.Gateway
{
    public class GatewayOptions
    {
        public GatewayOptions(params IServiceRouter[] serviceRouters)
            : this(serviceRouters.AsEnumerable())
        {
        }

        public GatewayOptions(IEnumerable<IServiceRouter> serviceRouters)
        {
            if (serviceRouters == null)
            {
                throw new ArgumentNullException(nameof(serviceRouters));
            }

            ServiceRouters = serviceRouters.ToArray();
        }

        public IReadOnlyCollection<IServiceRouter> ServiceRouters { get; }
    }
}
