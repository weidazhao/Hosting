using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            app.Map("/sms", subApp =>
            {
                subApp.UseRouter(new ServiceRouter(Singleton<SmsServiceDescription>.Instance));
            });

            //
            // Demostrates the scenarios of side by side versioning / multiple kinds of clients.
            //
            app.Map("/counter", subApp =>
            {
                subApp.UseRouter(new ServiceRouter(Singleton<CounterServiceDescription>.Instance));
            });

            app.Map("/Hosting/CounterService", subApp =>
            {
                subApp.UseRouter(new ServiceRouter(Singleton<CounterServiceDescription>.Instance));
            });
        }
    }
}
