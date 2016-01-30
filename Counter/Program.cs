using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.AspNetCore;
using System.Fabric;

namespace Counter
{
    public static class Program
    {
        public static IWebHost _webHost;

        public static void Main(string[] args)
        {
            _webHost = new WebHostBuilder().UseDefaultConfiguration(args)
                                           .UseStartup<Startup>()
                                           .UseServiceFabricEndpoint("CounterTypeEndpoint")
                                           .ConfigureServices(services => services.AddTransient<IStartupFilter, ServiceRepoStartupFilter>())
                                           .Build();

            using (var fabricRuntime = FabricRuntime.Create())
            {
                fabricRuntime.RegisterServiceType("CounterType", typeof(CounterService));

                _webHost.Run();
            }
        }
    }
}
