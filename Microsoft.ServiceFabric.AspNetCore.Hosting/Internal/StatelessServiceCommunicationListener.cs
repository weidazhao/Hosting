using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Features;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class StatelessServiceCommunicationListener : ICommunicationListener
    {
        private readonly IWebHost _webHost;
        private readonly IStatelessServiceInstance _instance;

        public StatelessServiceCommunicationListener(IWebHost webHost, IStatelessServiceInstance instance)
        {
            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _webHost = webHost;
            _instance = instance;
        }

        public void Abort()
        {
            StatelessServiceInstanceCache.Default.TryRemove(_instance);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            StatelessServiceInstanceCache.Default.TryRemove(_instance);

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            StatelessServiceInstanceCache.Default.TryAdd(_instance);

            var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

            return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses));
        }
    }
}
