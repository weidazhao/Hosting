using Microsoft.ServiceFabric.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;

namespace Gateway
{
    public class GatewayService : StatelessService
    {
        private readonly AspNetCoreCommunicationContext _context;

        public GatewayService(AspNetCoreCommunicationContext context)
        {
            _context = context;
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(_ => _context.CreateCommunicationListener(this)) };
        }
    }
}
