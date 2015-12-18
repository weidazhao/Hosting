using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListenerBuilder
    {
        private Type _startupType;
        private string[] _arguments;
        private string _endpointName;
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private ServiceInitializationParameters _serviceInitializationParameters;

        public AspNetCommunicationListenerBuilder UseStartup(Type startupType)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }

            _startupType = startupType;

            return this;
        }

        public AspNetCommunicationListenerBuilder UseArguments(string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            _arguments = arguments;

            return this;
        }

        public AspNetCommunicationListenerBuilder UseEndpoint(string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                throw new ArgumentException($"{nameof(endpointName)} can not be null.", nameof(endpointName));
            }

            _endpointName = endpointName;

            return this;
        }

        public AspNetCommunicationListenerBuilder UseService(Type serviceType, object serviceInstance)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (serviceInstance == null)
            {
                throw new ArgumentNullException(nameof(serviceInstance));
            }

            _services[serviceType] = serviceInstance;

            return this;
        }

        public ICommunicationListener Build(ServiceInitializationParameters serviceInitializationParameters)
        {
            if (serviceInitializationParameters == null)
            {
                throw new ArgumentNullException(nameof(serviceInitializationParameters));
            }

            if (_startupType == null)
            {
                throw new InvalidOperationException($"{nameof(_startupType)} can not be null.");
            }

            if (string.IsNullOrEmpty(_endpointName))
            {
                throw new InvalidOperationException($"{nameof(_endpointName)} can not be null or empty.");
            }

            _serviceInitializationParameters = serviceInitializationParameters;

            return new AspNetCommunicationListener(this);
        }

        private sealed class AspNetCommunicationListener : ICommunicationListener
        {
            private readonly AspNetCommunicationListenerBuilder _builder;

            private WebApplication2 _webApp;

            public AspNetCommunicationListener(AspNetCommunicationListenerBuilder builder)
            {
                if (builder == null)
                {
                    throw new ArgumentNullException(nameof(builder));
                }

                _builder = builder;
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
                var endpoint = _builder._serviceInitializationParameters.CodePackageActivationContext.GetEndpoint(_builder._endpointName);

                var serverUrl = $"{endpoint.Protocol}://+:{endpoint.Port}";

                var args = (_builder._arguments ?? Enumerable.Empty<string>()).Concat(new[] { "--server.urls", serverUrl }).ToArray();

                _webApp = new WebApplication2(_builder._startupType, ConfigureServices, args);

                return Task.FromResult(serverUrl);
            }

            private void ConfigureServices(IServiceCollection serviceCollection)
            {
                foreach (var service in _builder._services)
                {
                    serviceCollection.AddInstance(service.Key, service.Value);
                }
            }
        }
    }
}
