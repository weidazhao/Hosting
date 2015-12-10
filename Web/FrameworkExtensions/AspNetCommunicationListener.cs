using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListener<TStartup> : ICommunicationListener
    {
        private const string HostingJsonFile = "hosting.json";
        private const string ConfigFileKey = "config";

        private ServiceInitializationParameters _initializationParameters;
        private string[] _args;

        private IApplication _application;

        public AspNetCommunicationListener(ServiceInitializationParameters initializationParameters, string[] args)
        {
            _initializationParameters = initializationParameters;
            _args = args;
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
            var tempBuilder = new ConfigurationBuilder().AddCommandLine(_args);
            var tempConfig = tempBuilder.Build();
            var configFilePath = tempConfig[ConfigFileKey] ?? HostingJsonFile;

            var config = new ConfigurationBuilder().AddJsonFile(configFilePath, optional: true)
                                                   .AddEnvironmentVariables()
                                                   .AddCommandLine(_args)
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

        private string ResolveServerUrl()
        {
            var endpointName = $"{PlatformServices.Default.Application.ApplicationName}TypeEndpoint";

            var endpoint = _initializationParameters.CodePackageActivationContext.GetEndpoint(endpointName);

            return $"{endpoint.Protocol}://+:{endpoint.Port}";
        }
    }
}
