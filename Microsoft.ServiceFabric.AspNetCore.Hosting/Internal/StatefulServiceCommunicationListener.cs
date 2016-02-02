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
    public class StatefulServiceCommunicationListener : ICommunicationListener
    {
        private readonly IWebHost _webHost;
        private readonly IStatefulServiceReplica _replica;
        private PathString _replicaUrlPrefix;

        public StatefulServiceCommunicationListener(IWebHost webHost, IStatefulServiceReplica replica)
        {
            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            if (replica == null)
            {
                throw new ArgumentNullException(nameof(replica));
            }

            _webHost = webHost;
            _replica = replica;
        }

        public void Abort()
        {
            IStatefulServiceReplica replica;
            StatefulServiceReplicaCache.Default.TryRemove(_replicaUrlPrefix, out replica);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            IStatefulServiceReplica replica;
            StatefulServiceReplicaCache.Default.TryRemove(_replicaUrlPrefix, out replica);

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            StatefulServiceReplicaCache.Default.TryAdd(_replica, out _replicaUrlPrefix);

            var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

            return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses.Select(address => $"{address}{_replicaUrlPrefix}")));
        }
    }
}
