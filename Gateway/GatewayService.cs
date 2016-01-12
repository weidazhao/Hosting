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
            var webApp = new WebApplicationBuilder().UseConfiguration(WebApplicationConfiguration.GetDefault())
                                                    .UseStartup<Startup>()
                                                    .Build();

            // Replace the address with the one dynamically allocated by Service Fabric.
            string listeningAddress = AspNetCommunicationListener.GetListeningAddress(ServiceInitializationParameters, "GatewayTypeEndpoint");
            webApp.GetAddresses().Clear();
            webApp.GetAddresses().Add(listeningAddress);

            return new[] { new ServiceInstanceListener(_ => new AspNetCommunicationListener(webApp)) };
        }
    }
}
