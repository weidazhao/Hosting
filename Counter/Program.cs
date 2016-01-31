using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore;
using System.Fabric;

namespace Counter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                              .UseStartup<Startup>()
                                              .UseServiceFabric(new ServiceFabricOptions("CounterTypeEndpoint") { ServiceType = typeof(ICounterService) })
                                              .Build();

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("CounterType", () => new CounterService(webHost));

                webHost.Run();
            }
        }
    }
}
