using Microsoft.ServiceFabric.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Fabric;

namespace Gateway
{
    public class GatewayService : StatelessService
    {
        private readonly AspNetCoreCommunicationContext _communicationContext;

        public GatewayService(StatelessServiceContext serviceContext, AspNetCoreCommunicationContext communicationContext)
            : base(serviceContext)
        {
            _communicationContext = communicationContext;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(_ => _communicationContext.CreateCommunicationListener(this)) };
        }
    }
}
