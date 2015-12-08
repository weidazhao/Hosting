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
    public class HttpCommunicationListener<TStartup> : ICommunicationListener
    {
        private const string HostingJsonFile = "hosting.json";
        private const string ConfigFileKey = "config";

        private readonly ServiceInitializationParameters _initializationParameters;

        private TaskCompletionSource<string> _appStarted = new TaskCompletionSource<string>();
        private TaskCompletionSource<bool> _appStopped = new TaskCompletionSource<bool>();
        private IApplication _app;

        public HttpCommunicationListener(ServiceInitializationParameters initializationParameters)
        {
            _initializationParameters = initializationParameters;
        }

        public void Abort()
        {
            if (_app != null)
            {
                var lifetimeService = _app.Services.GetService(typeof(IApplicationLifetime)) as IApplicationLifetime;

                lifetimeService.StopApplication();

                _appStopped.Task.GetAwaiter().GetResult();

                _app.Dispose();

                _app = null;
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            if (_app != null)
            {
                var lifetimeService = _app.Services.GetService(typeof(IApplicationLifetime)) as IApplicationLifetime;

                lifetimeService.StopApplication();

                await _appStopped.Task;

                _app.Dispose();

                _app = null;
            }
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            // Allow the location of the json file to be specified via a --config command line arg
            var tempBuilder = new ConfigurationBuilder().AddCommandLine(Program.Arguments);
            var tempConfig = tempBuilder.Build();
            var configFilePath = tempConfig[ConfigFileKey] ?? HostingJsonFile;

            var config = new ConfigurationBuilder()
                .AddJsonFile(configFilePath, optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(Program.Arguments)
                .Build();

            var endpointName = $"{PlatformServices.Default.Application.ApplicationName}TypeEndpoint";
            var endpoint = _initializationParameters.CodePackageActivationContext.GetEndpoint(endpointName);
            config["server.urls"] = $"{endpoint.Protocol}://+:{endpoint.Port}";

            var hostBuilder = new WebHostBuilder(config, captureStartupErrors: true);
            hostBuilder.UseStartup(typeof(TStartup));

            var host = hostBuilder.Build();

            _app = host.Start();

            var appLifetime = _app.Services.GetService(typeof(IApplicationLifetime)) as IApplicationLifetime;
            appLifetime.ApplicationStarted.Register(() => _appStarted.TrySetResult(config["server.urls"]));

            appLifetime.ApplicationStopped.Register(() => _appStopped.TrySetResult(true));

            return _appStarted.Task;
        }
    }
}
