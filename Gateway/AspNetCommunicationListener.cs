using Microsoft.AspNet.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway
{
    public class AspNetCommunicationListener : ICommunicationListener
    {
        private IWebApplication _webApp;
        private IDisposable _token;

        public AspNetCommunicationListener(IWebApplication webApp)
        {
            _webApp = webApp;
        }

        public void Abort()
        {
            _token?.Dispose();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _token?.Dispose();

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            _token = _webApp.Start();

            return Task.FromResult(_webApp.GetAddresses().First().Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN));
        }
    }
}
