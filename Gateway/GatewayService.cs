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
            // Get the address dynamically allocated by Service Fabric.
            string listeningAddress = AddressUtilities.GetListeningAddress(ServiceInitializationParameters, "GatewayTypeEndpoint");

            // Build an ASP.NET 5 web application that serves as the communication listener.
            var webHostBuilder = new WebHostBuilder().UseDefaultConfiguration()
                                                     .UseStartup<Startup>()
                                                     .UseUrls(listeningAddress);

            return new[] { new ServiceInstanceListener(_ => new AspNetCommunicationListener(webHostBuilder, AddressUtilities.GetPublishingAddress(listeningAddress))) };
        }
    }
}
