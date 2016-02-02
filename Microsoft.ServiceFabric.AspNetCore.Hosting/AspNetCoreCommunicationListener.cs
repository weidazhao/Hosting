using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting.Internal;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
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
    }
}
