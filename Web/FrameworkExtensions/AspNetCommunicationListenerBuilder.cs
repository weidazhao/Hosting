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
        internal ServiceInitializationParameters ServiceInitializationParameters { get; private set; }

        public AspNetCommunicationListenerBuilder UseStartupType(Type startupType)
        {
            if (startupType == null)
            {
                throw new ArgumentNullException(nameof(startupType));
            }

            StartupType = startupType;

            return this;
        }

        public AspNetCommunicationListenerBuilder UseArguments(string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            Arguments = arguments;

            return this;
        }

        public AspNetCommunicationListenerBuilder UseEndpoint(string endpointName)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                throw new ArgumentException($"{nameof(endpointName)} can not be null.", nameof(endpointName));
            }

            EndpointName = endpointName;

            return this;
        }

        public AspNetCommunicationListenerBuilder UseService(Type serviceType, object serviceInstance)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (serviceInstance == null)
            {
                throw new ArgumentNullException(nameof(serviceInstance));
            }

            Services[serviceType] = serviceInstance;

            return this;
        }

        public AspNetCommunicationListener Build(ServiceInitializationParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (StartupType == null)
            {
                throw new InvalidOperationException($"{nameof(StartupType)} can not be null.");
            }

            if (string.IsNullOrEmpty(EndpointName))
            {
                throw new InvalidOperationException($"{nameof(EndpointName)} can not be null or empty.");
            }

            return new AspNetCommunicationListener(this);
        }
    }
}
