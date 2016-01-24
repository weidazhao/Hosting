using Microsoft.AspNet.Hosting;
using Microsoft.ServiceFabric.AspNet;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;

namespace Gateway
{
    public class GatewayService : StatelessService
    {
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            // Build an ASP.NET 5 web application that serves as the communication listener.
            var webHost = new WebHostBuilder().UseDefaultConfiguration()
                                              .UseStartup<Startup>()
                                              .UseServiceFabricEndpoint(ServiceInitializationParameters, "GatewayTypeEndpoint")
                                              .Build();

            return new[] { new ServiceInstanceListener(_ => new AspNetCommunicationListener(webHost)) };
        }
    }
}
