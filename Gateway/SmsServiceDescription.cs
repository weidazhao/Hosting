using Microsoft.AspNetCore.Http;
using Microsoft.ServiceFabric.AspNetCore.Gateway;
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

        public override Task<long> ComputeUniformInt64PartitionKeyAsync(HttpContext context)
        {
            try
            {
                var pathSegments = context.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                // Assumes that the last two segments in the path are {user}/{message}
                string user = pathSegments[pathSegments.Length - 2];

                return Task.FromResult((long)user.GetHashCode());
            }
            catch
            {
                // Wrong, but ok for prototype
                return Task.FromResult((long)context.GetHashCode());
            }
        }
    }
}
