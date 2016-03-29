﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Counter
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var communicationContext = CreateAspNetCoreCommunicationContext(args);

            ServiceRuntime.RegisterServiceAsync("CounterType", serviceContext => new CounterService(serviceContext, communicationContext)).GetAwaiter().GetResult();

            communicationContext.WebHost.Run();
        }

        private static AspNetCoreCommunicationContext CreateAspNetCoreCommunicationContext(string[] args)
        {
            var webHost = new WebHostBuilder().UseDefaultHostingConfiguration(args)
                                              .UseStartup<Startup>()
                                              .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                                              .UseServiceFabricEndpoint("CounterTypeEndpoint")
                                              .Build();

            return new AspNetCoreCommunicationContext(webHost);
        }
    }
}
