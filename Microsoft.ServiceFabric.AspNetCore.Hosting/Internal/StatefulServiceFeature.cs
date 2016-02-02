using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class StatefulServiceFeature
    {
        public IStatefulServiceReplica Replica { get; set; }
    }
}
