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
            var context = CreateAspNetCoreCommunicationContext(args);

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("CounterType", () => new CounterService(context));

                context.WebHost.Run();
            }
        }

        private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext(string[] args)
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

            var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                              .UseStartup<Startup>()
                                              .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                                              .UseServiceFabric(options)
                                              .Build();

            return new AspNetCoreCommunicationContext(webHost, addUrlPrefix: true);
        }
    }
}
