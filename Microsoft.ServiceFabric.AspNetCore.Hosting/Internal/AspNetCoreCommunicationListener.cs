using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class AspNetCoreCommunicationListener : ICommunicationListener
    {
        private readonly AspNetCoreCommunicationContext _context;
        private readonly object _service;
        private readonly ServiceFabricServiceRegistry _registry;

        private PathString _servicePathBase;

        public AspNetCoreCommunicationListener(AspNetCoreCommunicationContext context, StatelessService service)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _registry = _context.WebHost.Services.GetService<ServiceFabricServiceRegistry>();
        }

        public AspNetCoreCommunicationListener(AspNetCoreCommunicationContext context, StatefulService service)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _registry = _context.WebHost.Services.GetService<ServiceFabricServiceRegistry>();
        }

        #region ICommunicationListener

        public void Abort()
        {
            if (_registry != null)
            {
                if (!_registry.TryRemove(_servicePathBase, out object service))
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            if (_registry != null)
            {
                if (!_registry.TryRemove(_servicePathBase, out object service))
                {
                    throw new InvalidOperationException();
                }
            }

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            string ipAddressOrFQDN = FabricRuntime.GetNodeContext().IPAddressOrFQDN;

            var serverAddressesFeature = _context.WebHost.ServerFeatures.Get<IServerAddressesFeature>();

            IEnumerable<string> addresses = serverAddressesFeature.Addresses.Select(address => address.Replace("+", ipAddressOrFQDN));

            if (_registry != null)
            {
                if (!_registry.TryAdd(_service, out _servicePathBase))
                {
                    throw new InvalidOperationException();
                }

                addresses = addresses.Select(address => $"{address}{_servicePathBase}");
            }

            return Task.FromResult(string.Join(";", addresses.Distinct()));
        }

        #endregion ICommunicationListener
    }
}
