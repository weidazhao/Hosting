using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using System.IO;
using System.Threading.Tasks;

namespace Sms
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            var communicationContext = CreateAspNetCoreCommunicationContext();

            await communicationContext.WebHost.StartAsync();

            await ServiceRuntime.RegisterServiceAsync("SmsType", serviceContext => new SmsService(serviceContext, communicationContext));

            await communicationContext.WebHost.WaitForShutdownAsync();
        }

        private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext()
        {
            var webHost = new WebHostBuilder().UseKestrel()
                                              .UseContentRoot(Directory.GetCurrentDirectory())
                                              .UseStartup<Startup>()
                                              .UseServiceFabricEndpoint("SmsTypeEndpoint")
                                              .Build();

            return new AspNetCoreCommunicationContext(webHost);
        }
    }
}
