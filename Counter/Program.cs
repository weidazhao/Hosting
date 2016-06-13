using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using System.IO;

namespace Counter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var communicationContext = CreateAspNetCoreCommunicationContext();

            ServiceRuntime.RegisterServiceAsync("CounterType", serviceContext => new CounterService(serviceContext, communicationContext)).GetAwaiter().GetResult();

            communicationContext.WebHost.Run();
        }

        private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext()
        {
            var webHost = new WebHostBuilder().UseWebListener()
                                              .UseContentRoot(Directory.GetCurrentDirectory())
                                              .UseStartup<Startup>()
                                              .UseServiceFabricEndpoint("CounterTypeEndpoint")
                                              .Build();

            return new AspNetCoreCommunicationContext(webHost);
        }
    }
}
