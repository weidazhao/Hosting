using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using System.Fabric;

namespace Gateway
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var context = CreateAspNetCoreCommunicationContext(args);

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatelessServiceFactory("GatewayType", () => new GatewayService(context));

                context.WebHost.Run();
            }
        }

        private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext(string[] args)
        {
            var options = new ServiceFabricOptions()
            {
                EndpointName = "GatewayTypeEndpoint"
            };

            var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                              .UseStartup<Startup>()
                                              .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                                              .UseServiceFabric(options)
                                              .Build();

            return new AspNetCoreCommunicationContext(webHost, addUrlPrefix: false);
        }
    }
}
