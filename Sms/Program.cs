using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using System.IO;

namespace Sms
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var communicationContext = CreateAspNetCoreCommunicationContext(args);

            ServiceRuntime.RegisterServiceAsync("SmsType", serviceContext => new SmsService(serviceContext, communicationContext)).GetAwaiter().GetResult();

            communicationContext.WebHost.Run();
        }

        private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext(string[] args)
        {
            var config = new ConfigurationBuilder().AddCommandLine(args).Build();

            var webHost = new WebHostBuilder().UseConfiguration(config)
                                              .UseContentRoot(Directory.GetCurrentDirectory())
                                              .UseStartup<Startup>()
                                              .UseKestrel()
                                              .UseServiceFabricEndpoint("SmsTypeEndpoint")
                                              .Build();

            return new AspNetCoreCommunicationContext(webHost);
        }
    }
}
