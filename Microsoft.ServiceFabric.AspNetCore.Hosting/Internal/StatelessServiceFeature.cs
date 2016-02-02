using System.Fabric;

namespace Microsoft.ServiceFabric.AspNetCore.Hosting.Internal
{
    public class StatelessServiceFeature
    {
        public IStatelessServiceInstance Instance { get; set; }
    }
}
