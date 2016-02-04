using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting.Internal;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class AspNetCoreCommunicationContext
    {
        public AspNetCoreCommunicationContext(IWebHost webHost, bool addUrlPrefix)
        {
            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            WebHost = webHost;
            AddUrlPrefix = addUrlPrefix;
        }

        public IWebHost WebHost { get; }

        public bool AddUrlPrefix { get; }

        public ICommunicationListener CreateCommunicationListener(IStatelessServiceInstance instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return new AspNetCoreCommunicationListener(instance, WebHost, AddUrlPrefix);
        }

        public ICommunicationListener CreateCommunicationListener(IStatefulServiceReplica replica)
        {
            if (replica == null)
            {
                throw new ArgumentNullException(nameof(replica));
            }

            return new AspNetCoreCommunicationListener(replica, WebHost, AddUrlPrefix);
        }
    }
}
