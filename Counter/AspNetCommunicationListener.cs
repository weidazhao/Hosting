using Microsoft.AspNet.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ServiceFabric.AspNet
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

            return Task.FromResult(GetPublishingAddress(_webApp.GetAddresses().First()));
        }

        public static string GetListeningAddress(ServiceInitializationParameters parameters, string endpointName)
        {
            var endpoint = parameters.CodePackageActivationContext.GetEndpoint(endpointName);

            if (parameters is StatefulServiceInitializationParameters)
            {
                var statefulInitParams = (StatefulServiceInitializationParameters)parameters;

                return $"{endpoint.Protocol}://+:{endpoint.Port}/{statefulInitParams.PartitionId}/{statefulInitParams.ReplicaId}/{Guid.NewGuid()}";
            }
            else if (parameters is StatelessServiceInitializationParameters)
            {
                return $"{endpoint.Protocol}://+:{endpoint.Port}";
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static string GetPublishingAddress(ServiceInitializationParameters parameters, string endpointName)
        {
            return GetPublishingAddress(GetListeningAddress(parameters, endpointName));
        }

        public static string GetPublishingAddress(string listeningAddress)
        {
            return listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
        }
    }
}
