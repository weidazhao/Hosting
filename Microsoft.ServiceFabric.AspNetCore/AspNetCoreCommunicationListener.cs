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
        private readonly Guid _id;
        private readonly object _service;
        private readonly IWebHost _webHost;

        public AspNetCoreCommunicationListener(object service, IWebHost webHost)
        {
            _id = Guid.NewGuid();
            _service = service;
            _webHost = webHost;
        }

        public void Abort()
        {
            ServiceRepo.Instance.RemoveService(_id);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            ServiceRepo.Instance.RemoveService(_id);

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            ServiceRepo.Instance.AddService(_id, _service);

            var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

            if (_service is IStatelessServiceInstance)
            {
                return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses));
            }
            else
            {
                return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses.Select(address => $"{address}/{_id}")));
            }
        }
    }
}
