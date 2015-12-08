using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Web
{
    public class HttpCommunicationListener : ICommunicationListener
    {
        public void Abort()
        {
            throw new NotImplementedException();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
