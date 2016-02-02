using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly ICommunicationListener _impl;

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

            if (instanceOrReplica is IStatelessServiceInstance)
            {
                _impl = new StatelessServiceCommunicationListener(webHost, (IStatelessServiceInstance)instanceOrReplica);
            }
            else if (instanceOrReplica is IStatefulServiceReplica)
            {
                _impl = new StatefulServiceCommunicationListener(webHost, (IStatefulServiceReplica)instanceOrReplica);
            }
            else
            {
                throw new ArgumentException(null, nameof(instanceOrReplica));
            }
        }

        public void Abort()
        {
            _impl.Abort();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return _impl.CloseAsync(cancellationToken);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            return _impl.OpenAsync(cancellationToken);
        }

        private class StatelessServiceCommunicationListener : ICommunicationListener
        {
            private readonly IWebHost _webHost;
            private readonly IStatelessServiceInstance _instance;

            public StatelessServiceCommunicationListener(IWebHost webHost, IStatelessServiceInstance instance)
            {
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

        private class StatefulServiceCommunicationListener : ICommunicationListener
        {
            private readonly IWebHost _webHost;
            private readonly IStatefulServiceReplica _replica;
            private PathString _replicaUrlPrefix;

            public StatefulServiceCommunicationListener(IWebHost webHost, IStatefulServiceReplica replica)
            {
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
}
