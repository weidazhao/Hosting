using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore;
using System.Fabric;

namespace Gateway
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                              .UseStartup<Startup>()
                                              .UseServiceFabric(endpointName: "GatewayTypeEndpoint")
                                              .Build();

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatelessServiceFactory("GatewayType", () => new GatewayService(webHost));

                webHost.Run();
            }
        }
    }
}
