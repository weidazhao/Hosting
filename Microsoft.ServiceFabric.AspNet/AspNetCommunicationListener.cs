using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Server.Features;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet
{
    public class AspNetCommunicationListener : ICommunicationListener
    {
        private readonly IWebHost _webHost;

        public AspNetCommunicationListener(IWebHost webHost)
        {
            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            _webHost = webHost;
        }

        public void Abort()
        {
            _webHost.Dispose();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _webHost.Dispose();

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            _webHost.Start();

            var serverAddressesFeature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

            return Task.FromResult(string.Join(";", serverAddressesFeature.Addresses));
        }
    }
}
