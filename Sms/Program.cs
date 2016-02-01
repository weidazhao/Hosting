using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using System.Fabric;

namespace Sms
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var webHost = BuildWebHost(args);

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterStatefulServiceFactory("SmsType", () => new SmsService(webHost));

                webHost.Run();
            }
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            var options = new ServiceFabricOptions()
            {
                EndpointName = "SmsTypeEndpoint",
                ServiceType = typeof(ISmsService)
            };

            return new WebHostBuilder().UseDefaultConfiguration(args)
                                       .UseStartup<Startup>()
                                       .UseServiceFabric(options)
                                       .Build();
        }
    }
}
