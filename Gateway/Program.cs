using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore;
using System.Fabric;

namespace Gateway
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var options = new ServiceFabricOptions()
            {
                EndpointName = "GatewayTypeEndpoint"
            };

            var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                              .UseStartup<Startup>()
                                              .UseServiceFabric(options)
                                              .Build();

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatelessServiceFactory("GatewayType", () => new GatewayService(webHost));

                webHost.Run();
            }
        }
    }
}
