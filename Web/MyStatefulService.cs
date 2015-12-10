using Microsoft.ServiceFabric.Services.Communication.AspNet;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;

namespace Web
{
    public class MyStatefulService : StatefulService
    {
        private string[] _args;

        public MyStatefulService(string[] args)
        {
            _args = args;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[] { new ServiceReplicaListener(p => new AspNetCommunicationListener<Startup>(p, _args)) };
        }
    }
}
