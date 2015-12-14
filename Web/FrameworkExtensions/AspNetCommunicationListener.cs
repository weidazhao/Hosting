using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListener : ICommunicationListener
    {
        private readonly string _serverUrl;
        private readonly Type _startupType;
        private readonly Action<IServiceCollection> _configureServices;
        private readonly string[] _args;

        private WebApplication2 _webApp;

        public AspNetCommunicationListener(string serverUrl, Type startupType, Action<IServiceCollection> configureServices, string[] args)
        {
            _serverUrl = serverUrl;
            _startupType = startupType;
            _configureServices = configureServices;
            _args = args;
        }

        public void Abort()
        {
            _webApp.Dispose();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            _webApp.Dispose();

            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            var args = (_args ?? Enumerable.Empty<string>()).Concat(new[] { "--server.urls", _serverUrl }).ToArray();

            _webApp = new WebApplication2(_startupType, _configureServices, args);

            return Task.FromResult(_serverUrl);
        }
    }
}
