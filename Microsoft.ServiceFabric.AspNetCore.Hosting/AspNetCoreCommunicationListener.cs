using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Features;
using Microsoft.ServiceFabric.AspNetCore.Hosting.Internal;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class AspNetCoreCommunicationListener : ICommunicationListener
    {
        private readonly object _instanceOrReplica;
        private readonly IWebHost _webHost;
        private readonly bool _withUrlPrefix;
        private PathString _urlPrefix;

        public AspNetCoreCommunicationListener(object instanceOrReplica, IWebHost webHost, bool withUrlPrefix)
        {
            if (instanceOrReplica == null)
            {
                throw new ArgumentNullException(nameof(instanceOrReplica));
            }

            if (!(instanceOrReplica is IStatelessServiceInstance) && !(instanceOrReplica is IStatefulServiceReplica))
            {
                throw new ArgumentException(null, nameof(instanceOrReplica));
            }

            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            _instanceOrReplica = instanceOrReplica;
            _webHost = webHost;
            _withUrlPrefix = withUrlPrefix;
        }

        public void Abort()
        {
            if (_withUrlPrefix)
            {
                object instanceOrReplica;
                ServiceFabricRegistry.Default.TryRemove(_urlPrefix, out instanceOrReplica);
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            if (_withUrlPrefix)
            {
                object instanceOrReplica;
                ServiceFabricRegistry.Default.TryRemove(_urlPrefix, out instanceOrReplica);
            }

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

            if (_withUrlPrefix)
            {
                ServiceFabricRegistry.Default.TryAdd(_instanceOrReplica, out _urlPrefix);
                return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses.Select(address => $"{address}{_urlPrefix}")));
            }

            return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses));
        }
    }
}
