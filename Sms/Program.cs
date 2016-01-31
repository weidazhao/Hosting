using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore;
using System.Fabric;

namespace Sms
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var options = new ServiceFabricOptions()
            {
                EndpointName = "SmsTypeEndpoint",
                ServiceType = typeof(ISmsService)
            };

            var webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                              .UseStartup<Startup>()
                                              .UseServiceFabric(options)
                                              .Build();

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("SmsType", () => new SmsService(webHost));

                webHost.Run();
            }
        }
    }
}
