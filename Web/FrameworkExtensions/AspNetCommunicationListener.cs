using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListener : ICommunicationListener
    {
        private readonly AspNetCommunicationListenerBuilder _builder;
        private readonly ServiceInitializationParameters _parameters;

        private WebApplication2 _webApp;

        internal AspNetCommunicationListener(AspNetCommunicationListenerBuilder builder, ServiceInitializationParameters parameters)
        {
            _builder = builder;
            _parameters = parameters;
        }

        void ICommunicationListener.Abort()
        {
            _webApp.Dispose();
        }

        Task ICommunicationListener.CloseAsync(CancellationToken cancellationToken)
        {
            _webApp.Dispose();

            return Task.FromResult(true);
        }

        Task<string> ICommunicationListener.OpenAsync(CancellationToken cancellationToken)
        {
            var endpoint = _parameters.CodePackageActivationContext.GetEndpoint(_builder.EndpointName);

            var serverUrl = $"{endpoint.Protocol}://+:{endpoint.Port}";

            var args = (_builder.Arguments ?? Enumerable.Empty<string>()).Concat(new[] { "--server.urls", serverUrl }).ToArray();

            _webApp = new WebApplication2(_builder.StartupType, ConfigureServices, args);

            return Task.FromResult(serverUrl);
        }

        private void ConfigureServices(IServiceCollection serviceCollection)
        {
            foreach (var service in _builder.Services)
            {
                serviceCollection.AddInstance(service.Key, service.Value);
            }
        }
    }
}
