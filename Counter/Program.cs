using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using System.Collections.Immutable;
using System.Fabric;

namespace Counter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var webHost = BuildWebHost(args);

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("CounterType", () => new CounterService(webHost));

                webHost.Run();
            }
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            var serviceDescription = new ServiceDescription()
            {
                ServiceType = typeof(CounterService),
                InterfaceTypes = ImmutableArray.Create(typeof(ICounterService))
            };

            var options = new ServiceFabricOptions()
            {
                EndpointName = "CounterTypeEndpoint",
                ServiceDescriptions = ImmutableArray.Create(serviceDescription)
            };

            return new WebHostBuilder().UseDefaultConfiguration(args)
                                       .UseStartup<Startup>()
                                       .UseServiceFabric(options)
                                       .Build();
        }
    }
}
