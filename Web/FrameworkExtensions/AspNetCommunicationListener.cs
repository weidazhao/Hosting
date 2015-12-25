using Microsoft.AspNet.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListener : ICommunicationListener
    {
        private IWebApplication _webApp;
        private IDisposable _disposable;

        public AspNetCommunicationListener(IWebApplication webApp)
        {
            _webApp = webApp;
        }

        public void Abort()
        {
            _disposable?.Dispose();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _disposable?.Dispose();

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            _disposable = _webApp.Start();

            return Task.FromResult(_webApp.GetAddresses().First());
        }
    }
}
