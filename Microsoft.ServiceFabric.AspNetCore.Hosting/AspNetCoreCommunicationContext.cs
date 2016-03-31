using Microsoft.AspNetCore.Hosting;
using Microsoft.ServiceFabric.AspNetCore.Hosting.Internal;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class AspNetCoreCommunicationContext
    {
        public AspNetCoreCommunicationContext(IWebHost webHost)
        {
            if (webHost == null)
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            WebHost = webHost;
        }

        public IWebHost WebHost { get; }

        public ICommunicationListener CreateCommunicationListener(StatelessService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return new AspNetCoreCommunicationListener(this, service);
        }

        public ICommunicationListener CreateCommunicationListener(StatefulService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return new AspNetCoreCommunicationListener(this, service);
        }
    }
}
