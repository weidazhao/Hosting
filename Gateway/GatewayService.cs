using Microsoft.ServiceFabric.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;

namespace Gateway
{
    public class GatewayService : StatelessService
    {
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(_ => new AspNetCoreCommunicationListener(Program._webHost, this)) };
        }
    }
}
