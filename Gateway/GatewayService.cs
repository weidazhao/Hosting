using Microsoft.AspNetCore.Hosting;
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
            // Build an ASP.NET Core web application that serves as a communication listener.
            var webHost = new WebHostBuilder().UseDefaultConfiguration()
                                              .UseStartup<Startup>()
                                              .UseServiceFabricEndpoint(ServiceInitializationParameters, "GatewayTypeEndpoint")
                                              .Build();

            return new[] { new ServiceInstanceListener(_ => new AspNetCoreCommunicationListener(webHost)) };
        }
    }
}
