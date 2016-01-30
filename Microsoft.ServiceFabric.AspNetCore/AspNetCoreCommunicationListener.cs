using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Features;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNetCore
{
    public class AspNetCoreCommunicationListener : ICommunicationListener
    {
        private readonly ICommunicationListener _impl;

        public AspNetCoreCommunicationListener(IWebHost webHost, IStatelessServiceInstance instance)
        {
            _impl = new StatelessServiceInstanceCommunicationListener(webHost, instance);
        }

        public AspNetCoreCommunicationListener(IWebHost webHost, IStatefulServiceReplica replica)
        {
            _impl = new StatefulServiceReplicaCommunicationListener(webHost, replica);
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

        private class StatelessServiceInstanceCommunicationListener : ICommunicationListener
        {
            private readonly IWebHost _webHost;
            private readonly IStatelessServiceInstance _instance;

            public StatelessServiceInstanceCommunicationListener(IWebHost webHost, IStatelessServiceInstance instance)
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
            }

            public Task CloseAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }

            public Task<string> OpenAsync(CancellationToken cancellationToken)
            {
                var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

                return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses));
            }
        }

        private class StatefulServiceReplicaCommunicationListener : ICommunicationListener
        {
            private readonly IWebHost _webHost;
            private readonly IStatefulServiceReplica _replica;

            public StatefulServiceReplicaCommunicationListener(IWebHost webHost, IStatefulServiceReplica replica)
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
                UrlPrefixRegistry.Default.Unregister(_replica);
            }

            public Task CloseAsync(CancellationToken cancellationToken)
            {
                UrlPrefixRegistry.Default.Unregister(_replica);

                return Task.FromResult(true);
            }

            public Task<string> OpenAsync(CancellationToken cancellationToken)
            {
                var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

                string urlPrefix = UrlPrefixRegistry.Default.Register(_replica);

                return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses.Select(address => $"{address}{urlPrefix}")));
            }
        }
    }
}
