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
            var pathSegments = context.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string user = null;

            if (StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "GET"))
            {
                user = pathSegments[pathSegments.Length - 1];
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(context.Request.Method, "POST"))
            {
                user = pathSegments[pathSegments.Length - 2];
            }

            return Task.FromResult(HashCodeUtilities.GetInt64HashCode(user));
        }
    }
}
