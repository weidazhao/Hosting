using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore;
using System.Fabric;

namespace Counter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var options = new ServiceFabricOptions()
            {
                EndpointName = "CounterTypeEndpoint",
                ServiceType = typeof(ICounterService)
            };

            var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                              .UseStartup<Startup>()
                                              .UseServiceFabric(options)
                                              .Build();

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("CounterType", () => new CounterService(webHost));

                webHost.Run();
            }
        }
    }
}
