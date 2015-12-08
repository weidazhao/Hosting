using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Web
{
    public class MyStatefulService : StatefulService
    {
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return base.CreateServiceReplicaListeners();
        }
    }
}
