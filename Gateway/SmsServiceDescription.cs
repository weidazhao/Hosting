using Microsoft.ServiceFabric.AspNet.Gateway;
using System;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway
{
    public class SmsServiceDescription : ServiceDescription
    {
        public SmsServiceDescription()
            : base(new Uri("fabric:/Hosting/SmsService", UriKind.Absolute), ServicePartitionKind.Int64Range)
        {
        }

        public override Task<string> ComputeNamedPartitionKeyAsync(HttpRequestMessage request)
        {
            //
            // TODO
            // Override the method to provide custom logic for computing partition key.
            //
            return base.ComputeNamedPartitionKeyAsync(request);
        }
    }
}
