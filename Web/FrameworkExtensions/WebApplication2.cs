using Microsoft.AspNet.Hosting.Internal;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace Microsoft.AspNet.Hosting
{
    //
    // Copied from https://github.com/aspnet/Hosting/blob/dev/src/Microsoft.AspNet.Hosting/WebApplication.cs with modifications.
    //
    public class WebApplication2 : IDisposable
    {
        private const string HostingJsonFile = "hosting.json";
        private const string EnvironmentVariablesPrefix = "ASPNET_";
        private const string ConfigFileKey = "config";

        private IApplication _application;

        public WebApplication2(Type startupType, Action<IServiceCollection> configureServices, string[] args)
        {
            // Allow the location of the json file to be specified via a --config command line arg
            var tempBuilder = new ConfigurationBuilder().AddCommandLine(args);
            var tempConfig = tempBuilder.Build();
            var configFilePath = tempConfig[ConfigFileKey] ?? HostingJsonFile;
            var config = LoadHostingConfiguration(configFilePath, args);

            var hostBuilder = new WebHostBuilder(config, captureStartupErrors: true);

            if (startupType != null)
            {
                hostBuilder.UseStartup(startupType);
            }

            if (configureServices != null)
            {
                hostBuilder.UseServices(configureServices);
            }

            var host = hostBuilder.Build();
            _application = host.Start();

            var hostingEnv = _application.Services.GetRequiredService<IHostingEnvironment>();
            Console.WriteLine("Hosting environment: " + hostingEnv.EnvironmentName);

            var serverAddresses = _application.ServerFeatures.Get<IServerAddressesFeature>();
            if (serverAddresses != null)
            {
                foreach (var address in serverAddresses.Addresses)
                {
                    Console.WriteLine("Now listening on: " + address);
                }
            }
        }

        public void Dispose()
        {
            var application = Interlocked.Exchange(ref _application, null);

            if (application != null)
            {
                var appLifetime = application.Services.GetRequiredService<IApplicationLifetime>();
                appLifetime.StopApplication();

                application.Dispose();

                GC.SuppressFinalize(this);
            }
        }

        public static void Run(string[] args)
        {
            Run(startupType: null, args: args);
        }

        public static void Run<TStartup>()
        {
            Run(typeof(TStartup), null);
        }

        public static void Run<TStartup>(string[] args)
        {
            Run(typeof(TStartup), args);
        }

        public static void Run(Type startupType)
        {
            Run(startupType, null);
        }

        public static void Run(Type startupType, string[] args)
        {
            var webApp = new WebApplication2(startupType, null, args);

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                webApp.Dispose();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };

            Console.WriteLine("Application started. Press Ctrl+C to shut down.");

            var appLifetime = webApp._application.Services.GetRequiredService<IApplicationLifetime>();
            appLifetime.ApplicationStopping.WaitHandle.WaitOne();
        }

        internal static IConfiguration LoadHostingConfiguration(string configJsonPath, string[] args)
        {
            // We are adding all environment variables first and then adding the ASPNET_ ones
            // with the prefix removed to unify with the command line and config file formats
            return new ConfigurationBuilder()
                .AddJsonFile(configJsonPath, optional: true)
                .AddEnvironmentVariables()
                .AddEnvironmentVariables(prefix: EnvironmentVariablesPrefix)
                .AddCommandLine(args)
                .Build();
        }
    }
}
