using Microsoft.AspNet.Http;
using Microsoft.ServiceFabric.AspNet.Gateway;
using System;
using System.Fabric;
using System.Threading.Tasks;

namespace Gateway
{
    public class SmsServiceDescription : ServiceDescription
    {
        public SmsServiceDescription()
            : base(new Uri("fabric:/Hosting/SmsService", UriKind.Absolute), ServicePartitionKind.Int64Range)
        {
        }

        public override Task<long> ComputeUniformInt64PartitionKeyAsync(HttpRequest request)
        {
            var pathSegments = request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Assumes that the last two segments in the path are {user}/{message}
            string user = pathSegments[pathSegments.Length - 2];

            return Task.FromResult((long)user.GetHashCode());
        }
    }
}
