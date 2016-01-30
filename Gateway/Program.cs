using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore;
using System.Fabric;

namespace Gateway
{
    public static class Program
    {
        public static IWebHost _webHost;

        public static void Main(string[] args)
        {
            _webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                           .UseStartup<Startup>()
                                           .UseServiceFabric("GatewayTypeEndpoint")
                                           .Build();

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterServiceType("GatewayType", typeof(GatewayService));

                _webHost.Run();
            }
        }
    }
}
