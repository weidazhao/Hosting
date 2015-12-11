using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListener<TStartup> : ICommunicationListener
    {
        private ServiceInitializationParameters _initializationParameters;
        private string[] _args;

        private WebApplication2 _webApp;

        public AspNetCommunicationListener(ServiceInitializationParameters initializationParameters, string[] args)
        {
            _initializationParameters = initializationParameters;
            _args = args;
        }

        public void Abort()
        {
            _webApp.Dispose();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _webApp.Dispose();

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serverUrl = ResolveServerUrl();

            _webApp = new WebApplication2(typeof(TStartup), _args.Concat(new[] { "--server.urls", serverUrl }).ToArray());

            return Task.FromResult(serverUrl);
        }

        private string ResolveServerUrl()
        {
            var endpointName = $"{PlatformServices.Default.Application.ApplicationName}TypeEndpoint";

            var endpoint = _initializationParameters.CodePackageActivationContext.GetEndpoint(endpointName);

            return $"{endpoint.Protocol}://+:{endpoint.Port}";
        }
    }
}
