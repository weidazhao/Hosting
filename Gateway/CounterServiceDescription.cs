using Microsoft.ServiceFabric.AspNetCore.Gateway;
using System;
using System.Fabric;

namespace Gateway
{
    public class CounterServiceDescription : ServiceDescription
    {
        public CounterServiceDescription()
            : base(new Uri("fabric:/Hosting/CounterService", UriKind.Absolute), ServicePartitionKind.Singleton)
        {
        }
    }
}
