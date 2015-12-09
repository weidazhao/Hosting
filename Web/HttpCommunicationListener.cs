using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Web
{
    //
    // TODO:
    // Refer to https://github.com/aspnet/Hosting/blob/release/src/Microsoft.AspNet.Hosting/WebApplication.cs
    // See if Microsoft.AspNet.Hosting.WebApplication can be refactored so that HttpCommunicationListener can reuse it.
    //
    public class HttpCommunicationListener<TStartup> : ICommunicationListener
    {
        private const string HostingJsonFile = "hosting.json";
        private const string ConfigFileKey = "config";

        private readonly ServiceInitializationParameters _initializationParameters;

        private IApplication _application;

        public HttpCommunicationListener(ServiceInitializationParameters initializationParameters)
        {
            _initializationParameters = initializationParameters;
        }

        public void Abort()
        {
            StopApplication();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            StopApplication();

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serverUrl = ResolveServerUrl();

            StartApplication(serverUrl);

            return Task.FromResult(serverUrl);
        }

        private void StartApplication(string serverUrl)
        {
            var tempBuilder = new ConfigurationBuilder().AddCommandLine(Program.Arguments);
            var tempConfig = tempBuilder.Build();
            var configFilePath = tempConfig[ConfigFileKey] ?? HostingJsonFile;

            var config = new ConfigurationBuilder().AddJsonFile(configFilePath, optional: true)
                                                   .AddEnvironmentVariables()
                                                   .AddCommandLine(Program.Arguments)
                                                   .Build();

            config["server.urls"] = serverUrl;

            var hostBuilder = new WebHostBuilder(config, captureStartupErrors: true);
            hostBuilder.UseStartup(typeof(TStartup));

            var host = hostBuilder.Build();

            _application = host.Start();
        }

        private void StopApplication()
        {
            if (_application != null)
            {
                var lifetimeService = _application.Services.GetService(typeof(IApplicationLifetime)) as IApplicationLifetime;

                lifetimeService.StopApplication();

                _application.Dispose();

                _application = null;
            }
        }

        //
        // TODO:
        // Allow users to plug in their own URL patterns.
        // Refer to https://github.com/Azure/servicefabric-samples/blob/master/samples/Services/VS2015/WordCount/WordCount.Common/OwinCommunicationListener.cs
        //
        private string ResolveServerUrl()
        {
            var endpointName = $"{PlatformServices.Default.Application.ApplicationName}TypeEndpoint";

            var endpoint = _initializationParameters.CodePackageActivationContext.GetEndpoint(endpointName);

            return $"{endpoint.Protocol}://+:{endpoint.Port}";
        }
    }
}
