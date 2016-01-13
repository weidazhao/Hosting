using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.AspNet.Gateway;
using System;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;

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
            app.Map("/sms", subApp =>
            {
                subApp.RunGateway(new IServiceRouter[]
                {
                    new ServiceRouter(new SmsServiceDescription())
                });
            });

            var counterServiceDescription = new CounterServiceDescription();

            app.Map("/counter", subApp =>
            {
                subApp.RunGateway(new IServiceRouter[]
                {
                    new ServiceRouter(counterServiceDescription)
                });
            });

            //
            // Demostrates the scenarios of side by side / multiple kinds of clients.
            //
            app.RunGateway(new IServiceRouter[]
            {
                new UrlPrefixtBasedServiceRouter(counterServiceDescription),
                new HeaderBasedServiceRouter(counterServiceDescription)
            });
        }

        private class CounterServiceDescription : ServiceDescription
        {
            public CounterServiceDescription()
                : base(new Uri("fabric:/Hosting/CounterService", UriKind.Absolute), ServicePartitionKind.Int64Range)
            {
            }

            public override Task<string> ComputeNamedPartitionKeyAsync(HttpRequestMessage request)
            {
                //
                // TODO
                // Override the method to provide custom logic for computing partition key.
                //
                return base.ComputeNamedPartitionKeyAsync(request);
            }
        }

        private class SmsServiceDescription : ServiceDescription
        {
            public SmsServiceDescription()
                : base(new Uri("fabric:/Hosting/SmsService", UriKind.Absolute), ServicePartitionKind.Int64Range)
            {
            }

            public override Task<string> ComputeNamedPartitionKeyAsync(HttpRequestMessage request)
            {
                //
                // TODO
                // Override the method to provide custom logic for computing partition key.
                //
                return base.ComputeNamedPartitionKeyAsync(request);
            }
        }
    }
}
