using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting
{
    public class StatefulServiceFeature
    {
        public IStatefulServiceReplica Replica { get; set; }
    }
}
