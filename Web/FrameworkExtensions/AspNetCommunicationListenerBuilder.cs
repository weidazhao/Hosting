using System;
using System.Collections.Generic;
using System.Fabric;

namespace Microsoft.ServiceFabric.Services.Communication.AspNet
{
    public class AspNetCommunicationListenerBuilder
    {
        internal Type StartupType { get; private set; }
        internal string[] Arguments { get; private set; }
        internal string EndpointName { get; private set; }
        internal Dictionary<Type, object> Services { get; } = new Dictionary<Type, object>();

        public AspNetCommunicationListenerBuilder UseStartupType(Type startupType)
        {
            StartupType = startupType;
            return this;
        }

        public AspNetCommunicationListenerBuilder UseArguments(string[] arguments)
        {
            Arguments = arguments;
            return this;
        }

        public AspNetCommunicationListenerBuilder UseEndpoint(string endpointName)
        {
            EndpointName = endpointName;
            return this;
        }

        public AspNetCommunicationListenerBuilder UseService(Type serviceType, object serviceInstance)
        {
            Services[serviceType] = serviceInstance;
            return this;
        }

        public AspNetCommunicationListener Build(ServiceInitializationParameters parameters)
        {
            if (StartupType == null)
            {
                throw new InvalidOperationException("StartupType can not be null.");
            }

            if (string.IsNullOrEmpty(EndpointName))
            {
                throw new InvalidOperationException("EndpointName can not be null.");
            }

            return new AspNetCommunicationListener(this, parameters);
        }
    }
}
