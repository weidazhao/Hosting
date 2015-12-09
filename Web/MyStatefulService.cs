using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;

namespace Web
{
    public class MyStatefulService : StatefulService
    {
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(initializationParameters => new HttpCommunicationListener<Startup>(initializationParameters)) };
        }
    }
}
