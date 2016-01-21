using Microsoft.AspNet.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet
{
    public class AspNetCommunicationListener : ICommunicationListener
    {
        private readonly IWebHost _webHost;
        private readonly string _publishingAddress;

        public AspNetCommunicationListener(IWebHostBuilder webHostBuilder, string publishingAddress)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            if (publishingAddress == null)
            {
                throw new ArgumentNullException(nameof(publishingAddress));
            }

            _webHost = webHostBuilder.Build();
            _publishingAddress = publishingAddress;
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

            return Task.FromResult(_publishingAddress);
        }
    }
}
