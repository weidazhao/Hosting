using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListener : ICommunicationListener
    {
        private readonly AspNetCommunicationListenerBuilder _builder;

        private WebApplication2 _webApp;

        internal AspNetCommunicationListener(AspNetCommunicationListenerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            _builder = builder;
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
            var endpoint = _builder.ServiceInitializationParameters.CodePackageActivationContext.GetEndpoint(_builder.EndpointName);

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
