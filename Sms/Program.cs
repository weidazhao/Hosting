using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using System.Collections.Immutable;
using System.Fabric;

namespace Sms
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var context = CreateAspNetCoreCommunicationContext(args);

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("SmsType", () => new SmsService(context));

                context.WebHost.Run();
            }
        }

        private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext(string[] args)
        {
            var serviceDescription = new ServiceDescription()
            {
                ServiceType = typeof(SmsService),
                InterfaceTypes = ImmutableArray.Create(typeof(ISmsService))
            };

            var options = new ServiceFabricOptions()
            {
                EndpointName = "SmsTypeEndpoint",
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
