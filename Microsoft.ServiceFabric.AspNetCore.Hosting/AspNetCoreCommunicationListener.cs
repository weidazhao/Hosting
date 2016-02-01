using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Features;
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
        private readonly IWebHost _webHost;
        private readonly object _instanceOrReplica;

        public AspNetCoreCommunicationListener(IWebHost webHost, object instanceOrReplica)
        {
            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            if (instanceOrReplica == null)
            {
                throw new ArgumentNullException(nameof(instanceOrReplica));
            }

            if (!(instanceOrReplica is IStatelessServiceInstance) && !(instanceOrReplica is IStatefulServiceReplica))
            {
                throw new ArgumentException(null, nameof(instanceOrReplica));
            }

            _webHost = webHost;
            _instanceOrReplica = instanceOrReplica;
        }

        public void Abort()
        {
            UrlPrefixRegistry.Default.Unregister(_instanceOrReplica);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            UrlPrefixRegistry.Default.Unregister(_instanceOrReplica);

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            string urlPrefix = UrlPrefixRegistry.Default.Register(_instanceOrReplica);

            var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

            return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses.Select(address => $"{address}{urlPrefix}")));
        }
    }
}
