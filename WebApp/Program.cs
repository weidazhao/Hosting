using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;
using System.IO;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var localWebHost = CreateLocalWebHost();
            IHostingEnvironment env = localWebHost.Services.GetService(typeof(IHostingEnvironment)) as IHostingEnvironment;

            if (env.IsDevelopment())
            {
                localWebHost.Run();
            }
            else
            {
                var communicationContext = new AspNetCoreCommunicationContext(CreateServiceFabricWebHost());

                ServiceRuntime.RegisterServiceAsync("WebAppType", serviceContext => new WebAppService(serviceContext, communicationContext)).GetAwaiter().GetResult();

                communicationContext.WebHost.Run();
            }
        }

        private static IWebHost CreateLocalWebHost()
        {
            return new WebHostBuilder().UseKestrel()
                                       .UseUrls("http://localhost:8001")
                                       .UseContentRoot(Directory.GetCurrentDirectory())
                                       .UseStartup("WebApp")
                                       .Build();
        }

        private static IWebHost CreateServiceFabricWebHost()
        {
            return new WebHostBuilder().UseKestrel()
                                       .UseContentRoot(Directory.GetCurrentDirectory())
                                       .UseStartup<Startup>()
                                       .UseServiceFabricEndpoint("WebAppTypeEndpoint")
                                       .Build();
        }
    }
}
