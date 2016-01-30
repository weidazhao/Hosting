using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore;
using System.Fabric;

namespace Sms
{
    public static class Program
    {
        public static IWebHost _webHost;

        public static void Main(string[] args)
        {
            _webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                           .UseStartup<Startup>()
                                           .UseServiceFabric("SmsTypeEndpoint")
                                           .Build();

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterServiceType("SmsType", typeof(SmsService));

                _webHost.Run();
            }
        }
    }
}
