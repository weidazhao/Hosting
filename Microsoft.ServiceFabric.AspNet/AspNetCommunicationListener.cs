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
        private readonly IWebHostBuilder _webHostBuilder;
        private IWebHost _webHost;

        public AspNetCommunicationListener(IWebHostBuilder webHostBuilder)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            _webHostBuilder = webHostBuilder;
        }

        public void Abort()
        {
            _webHost?.Dispose();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _webHost?.Dispose();

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            _webHost = _webHostBuilder.Build();

            _webHost.Start();

            var feature = _webHost.ServerFeatures.Get<IServerAddressesFeature>();

            return Task.FromResult(string.Join(";", feature.Addresses));
        }
    }
}
