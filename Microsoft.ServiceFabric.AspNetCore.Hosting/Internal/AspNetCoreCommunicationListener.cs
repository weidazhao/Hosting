using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Features;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class AspNetCoreCommunicationListener : ICommunicationListener
    {
        private readonly object _instanceOrReplica;
        private readonly IWebHost _webHost;
        private readonly bool _addUrlPrefix;
        private PathString _urlPrefix;

        public AspNetCoreCommunicationListener(IStatelessServiceInstance instance, IWebHost webHost, bool addUrlPrefix)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            _instanceOrReplica = instance;
            _webHost = webHost;
            _addUrlPrefix = addUrlPrefix;
        }

        public AspNetCoreCommunicationListener(IStatefulServiceReplica replica, IWebHost webHost, bool addUrlPrefix)
        {
            if (replica == null)
            {
                throw new ArgumentNullException(nameof(replica));
            }

            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            _instanceOrReplica = replica;
            _webHost = webHost;
            _addUrlPrefix = addUrlPrefix;
        }

        public void Abort()
        {
            if (_addUrlPrefix)
            {
                object instanceOrReplica;
                ServiceFabricRegistry.Default.TryRemove(_urlPrefix, out instanceOrReplica);
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            if (_addUrlPrefix)
            {
                object instanceOrReplica;
                ServiceFabricRegistry.Default.TryRemove(_urlPrefix, out instanceOrReplica);
            }

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

            if (_addUrlPrefix)
            {
                ServiceFabricRegistry.Default.TryAdd(_instanceOrReplica, out _urlPrefix);
                return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses.Select(address => $"{address}{_urlPrefix}")));
            }

            return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses));
        }
    }
}
