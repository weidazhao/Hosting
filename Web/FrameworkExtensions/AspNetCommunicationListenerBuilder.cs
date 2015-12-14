using Microsoft.Extensions.DependencyInjection;
using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListenerBuilder
    {
        public Type StartupType { get; set; }

        public string[] Arguments { get; set; }

        public string ServerUrl { get; set; }

        public string EndpointName { get; set; }

        public Action<IServiceCollection> ConfigureServices { get; set; }

        public AspNetCommunicationListener Build(ServiceInitializationParameters parameters)
        {
            if (StartupType == null)
            {
                throw new InvalidOperationException("StartupType can not be null.");
            }

            if (string.IsNullOrEmpty(ServerUrl) && (parameters == null || string.IsNullOrEmpty(EndpointName)))
            {
                throw new InvalidOperationException("Server URL can not be resolved.");
            }

            string serverUrl = ServerUrl;

            if (string.IsNullOrEmpty(serverUrl))
            {
                var endpoint = parameters.CodePackageActivationContext.GetEndpoint(EndpointName);

                serverUrl = $"{endpoint.Protocol}://+:{endpoint.Port}";
            }

            return new AspNetCommunicationListener(serverUrl, StartupType, ConfigureServices, Arguments);
        }
    }
}
