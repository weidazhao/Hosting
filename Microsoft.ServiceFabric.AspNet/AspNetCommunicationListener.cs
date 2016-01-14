using Microsoft.AspNet.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet
{
    public class AspNetCommunicationListener : ICommunicationListener
    {
        private readonly IWebApplication _webApp;
        private readonly string _publishingAddress;
        private IDisposable _token;

        public AspNetCommunicationListener(IWebApplication webApp, string publishingAddress)
        {
            _webApp = webApp;
            _publishingAddress = publishingAddress;
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

            return Task.FromResult(_publishingAddress);
        }
    }
}
