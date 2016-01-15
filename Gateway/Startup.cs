using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.ServiceFabric.AspNet.Gateway;

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
            //
            // Scenarios:
            // 1. Multiple services.
            // 2. Various versions or kinds of clients side by side.
            //

            app.Map("/sms",
                subApp =>
                {
                    subApp.RunGateway(new GatewayOptions() { ServiceDescription = new SmsServiceDescription() });
                }
            );

            app.Map("/counter",
                subApp =>
                {
                    subApp.RunGateway(new GatewayOptions() { ServiceDescription = new CounterServiceDescription() });
                }
            );

            app.Map("/Hosting/CounterService",
                subApp =>
                {
                    subApp.RunGateway(new GatewayOptions() { ServiceDescription = new CounterServiceDescription() });
                }
            );

            app.MapWhen(
                context =>
                {
                    StringValues serviceNames;

                    return context.Request.Headers.TryGetValue("SF-ServiceName", out serviceNames) &&
                           serviceNames.Count == 1 &&
                           serviceNames[0] == "fabric:/Hosting/CounterService";
                },
                subApp =>
                {
                    subApp.RunGateway(new GatewayOptions() { ServiceDescription = new CounterServiceDescription() });
                }
            );
        }
    }
}
