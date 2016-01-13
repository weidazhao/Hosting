using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.AspNet.Gateway;
using System;
using System.Fabric;

namespace Gateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json")
                                                      .AddEnvironmentVariables()
                                                      .Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var counterServiceName = new Uri("fabric:/Hosting/CounterService", UriKind.Absolute);

            app.RunGateway(new IServiceRouter[]
            {
                new UrlPrefixtBasedServiceRouter(counterServiceName, ServicePartitionKind.Int64Range),
                new HeaderBasedServiceRouter(counterServiceName, ServicePartitionKind.Int64Range)
            });
        }
    }
}
